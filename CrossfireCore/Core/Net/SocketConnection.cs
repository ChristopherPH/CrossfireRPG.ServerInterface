using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CrossfireCore.ServerInterface
{
    public class SocketConnection
    {
        static Logger _Logger = new Logger(nameof(SocketConnection));

        const string DefaultServerHost = "127.0.0.1";

        public TcpClient client
        {
            get => _client;
            private set
            {
                _client = value;
                if (_client != null)
                {
                    _stream = _client.GetStream();
                }
            }
        }
        TcpClient _client;
        NetworkStream _stream;

        public ConnectionStatuses ConnectionStatus { get; private set; } = ConnectionStatuses.Disconnected;
        public string Host { get; private set; } = string.Empty;
        public int Port { get; private set; } = 0;

        public bool Connect(string Host = DefaultServerHost, int Port = Defaults.ServerPort)
        {
            Disconnect();

            _client = new TcpClient
            {
                NoDelay = true
            };

            try
            {
                var asyncResult = _client.BeginConnect(Host, Port, ConnectCallback, _client);

                //just in case we connect right away!
                if (asyncResult.CompletedSynchronously)
                {
                    if (_client.Connected)
                    {
                        _stream = client.GetStream();

                        //check if there are problems with the stream
                        System.Diagnostics.Debug.Assert(_stream != null);
                        System.Diagnostics.Debug.Assert(_stream.CanRead);
                        System.Diagnostics.Debug.Assert(_stream.CanWrite);

                        //start waiting for data
                        WaitForBytes(new StateObject(client, _stream));

                        this.Host = Host;
                        this.Port = Port;

                        SetConnectionStatus(ConnectionStatuses.Connected);

                        return true;
                    }

                    return false;
                }
            }
            catch (SocketException ex)
            {
                OnError?.Invoke(this, new ConnectionErrorEventArgs()
                {
                    ErrorCode = ex.ErrorCode,
                    ErrorMessage = ex.Message,
                });

                //at this point, we didn't set our connection status,
                //and never got a valid socket, so just clean up
                //instead of disconnecting
                Cleanup();
                return false;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ConnectionErrorEventArgs()
                {
                    ErrorMessage = ex.Message
                });

                Cleanup();
                return false;
            }

            _Logger.Info("Connecting to {0}:{1}", Host, Port);

            this.Host = Host;
            this.Port = Port;

            SetConnectionStatus(ConnectionStatuses.Connecting);

            return true;
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            var client = ar.AsyncState as TcpClient;

            System.Diagnostics.Debug.Assert(client != null, "ConnectCallback: client is not a TcpClient");

            //if we have called disconnect() while trying to connect, ignore this ConnectCallback()
            if (_client == null)
                return;

            System.Diagnostics.Debug.Assert(client == _client, "ConnectCallback: Internal client mismatch");

            //ensure socket is valid before calling EndConnect()
            if (client.Client == null)
                return;

            try
            {
                client.EndConnect(ar);
            }
            catch (SocketException ex)
            {
                var msg = ex.Message;
                var code = ex.ErrorCode;

                switch (ex.ErrorCode)
                {
                    case 10060: //WSAETIMEDOUT
                        msg = "Timed out connecting to server";
                        break;

                    case 10061: //WSAECONNREFUSED
                        msg = "Server refused connection";
                        break;
                }

                OnError?.Invoke(this, new ConnectionErrorEventArgs()
                {
                    ErrorCode = ex.ErrorCode,
                    ErrorMessage = msg
                });

                Disconnect();
                return;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ConnectionErrorEventArgs()
                {
                    ErrorMessage = ex.Message
                });

                Disconnect();
                return;
            }

            if (!client.Connected)
            {
                Disconnect();
                return;
            }

            _stream = client.GetStream();

            //check if there are problems with the stream
            System.Diagnostics.Debug.Assert(_stream != null);
            System.Diagnostics.Debug.Assert(_stream.CanRead);
            System.Diagnostics.Debug.Assert(_stream.CanWrite);

            //start waiting for data
            WaitForBytes(new StateObject(client, _stream));

            //notify connected
            _Logger.Info("Connected");
            SetConnectionStatus(ConnectionStatuses.Connected);
        }

        private void Cleanup()
        {
            if (_client != null)
            {
                if (_client.Connected)
                    _client.Close();

                _client.Dispose();
                _client = null;
            }

            if (_stream != null)
            {
                // Closing the tcpClient instance does not close the network stream.
                _stream.Close();
                _stream = null;
            }

            this.Host = string.Empty;
            this.Port = 0;
        }

        public void Disconnect()
        {
            if ((_client != null) && _client.Connected)
            {
                _client.Close();
            }

            _Logger.Info("Disconnected");
            SetConnectionStatus(ConnectionStatuses.Disconnected);

            Cleanup();
        }


        public bool SendMessage(string Message)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(Message));

            var messageBytes = Encoding.ASCII.GetBytes(Message);

            return SendMessage(messageBytes);
        }

        public bool SendMessage(string Format, params object[] args)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(Format));

            var Message = string.Format(Format, args);

            System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(Message));

            var messageBytes = Encoding.ASCII.GetBytes(Message);

            return SendMessage(messageBytes);
        }

        public bool SendMessage(byte[] Message)
        {
            if ((_client == null) || (_stream == null) || (!_client.Connected))
            {
                Cleanup();
                return false;
            }

            if (!_stream.CanWrite)
            {
                Cleanup();
                return false;
            }

            var messageLength = Message.Length;

            var lengthBytes = new byte[2];
            lengthBytes[0] = (byte)((messageLength >> 8) & 0xFF);
            lengthBytes[1] = (byte)((messageLength) & 0xFF);

            _stream.BeginWrite(lengthBytes, 0, lengthBytes.Length, BeginSendCallback, _stream);
            _stream.BeginWrite(Message, 0, Message.Length, BeginSendCallback, _stream);

            _Logger.Debug("Write {0} bytes\n{1}", 
                messageLength, HexDump.Utils.HexDump(Message));

            return true;
        }

        private void BeginSendCallback(IAsyncResult ar)
        {
            var stream = ar.AsyncState as NetworkStream;

            System.Diagnostics.Debug.Assert(stream == _stream, "BeginSendCallback: Internal stream mismatch");

            //stream has been shut down
            //TODO: cleanup/disconnect here?
            if (!stream.CanRead)
                return;

            try
            {
                stream.EndWrite(ar);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ConnectionErrorEventArgs()
                {
                    ErrorMessage = ex.Message
                });

                Disconnect();
                return;
            }
        }

        private void WaitForBytes(StateObject so)
        {
            //client has been disconnected
            //TODO: cleanup/disconnect here?
            if (_client == null)
                return;

            //this happened when the game manager called Connection.Disconnect() on a protocol mismatch
            //as _client was null
            System.Diagnostics.Debug.Assert(so.client == _client, "WaitForBytes: Internal client mismatch");

            //stream has been shut down
            //TODO: cleanup/disconnect here?
            if (!so.stream.CanRead)
                return;

            //setup state
            so.wantLen = StateObject.BufferSize;
            so.bufferLen = 0;

            try
            {
                var asyncResult = so.stream.BeginRead(so.buffer, 0, so.wantLen, BeginReadCallback, so);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ConnectionErrorEventArgs()
                {
                    ErrorMessage = ex.Message
                });

                Disconnect();
            }
        }

        private void BeginReadCallback(IAsyncResult ar)
        {
            var so = ar.AsyncState as StateObject;

            System.Diagnostics.Debug.Assert(so != null);
            System.Diagnostics.Debug.Assert(so.client == _client);
            System.Diagnostics.Debug.Assert(so.stream == _stream);

            //stream has been shut down
            //TODO: cleanup/disconnect here?
            if (!so.stream.CanRead)
                return;

            int bytesRead;

            try
            {
                bytesRead = so.stream.EndRead(ar);
            }
            catch (System.IO.IOException ex)
            {
                if (ex.InnerException is SocketException se)
                {
                    switch (se.ErrorCode)
                    {
                        //Don't send a ConnectionErrorEventArgs on these error codes
                        case 10054: //WSAECONNRESET / Connection reset by peer.
                            break;

                        //use socket error message instead of System.IO.IOException error message
                        default:
                            OnError?.Invoke(this, new ConnectionErrorEventArgs()
                            {
                                ErrorCode = se.ErrorCode,
                                ErrorMessage = se.Message
                            });
                            break;
                    }
                }
                else
                {
                    OnError?.Invoke(this, new ConnectionErrorEventArgs()
                    {
                        ErrorMessage = ex.Message
                    });
                }

                Disconnect();
                return;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new ConnectionErrorEventArgs()
                {
                    ErrorMessage = ex.Message
                });

                Disconnect();
                return;
            }

            if (bytesRead == 0)
            {
                Disconnect();
                return;
            }

            _Logger.Debug("Read {0} bytes\n{1}", bytesRead,
                HexDump.Utils.HexDump(so.buffer, so.bufferLen, bytesRead));

            //collect buffer data
            so.bufferLen += bytesRead;

            var args = new ConnectionPacketEventArgs();
            args.Packet = new byte[bytesRead];
            Array.Copy(so.buffer, args.Packet, bytesRead);

            OnPacket?.Invoke(this, args);

            WaitForBytes(so);
        }


        public event EventHandler<ConnectionStatusEventArgs> OnStatusChanged;
        public event EventHandler<ConnectionPacketEventArgs> OnPacket;
        public event EventHandler<ConnectionErrorEventArgs> OnError;

        protected void SetConnectionStatus(ConnectionStatuses Status)
        {
            if (ConnectionStatus != Status)
            {
                ConnectionStatus = Status;

                OnStatusChanged?.Invoke(this, new ConnectionStatusEventArgs()
                {
                    Status = Status,
                });
            }
        }

        class StateObject
        {
            public StateObject(TcpClient client, NetworkStream stream)
            {
                this.client = client;
                this.stream = stream;
            }

            // Client socket.  
            public TcpClient client { get; }
            public NetworkStream stream { get; }

            // Size of receive buffer.  
            public const int BufferSize = 65536;

            // Receive buffer.  
            public byte[] buffer { get; } = new byte[BufferSize];
            public int bufferLen { get; set; } = 0;

            public int wantLen { get; set; }

            public bool waitingForHeader { get; set; }
        }

    }


    public enum ConnectionStatuses
    {
        Disconnected,
        Connected,
        Connecting
    }


    public class ConnectionPacketEventArgs
    {
        public byte[] Packet { get; set; }
    }

    public class ConnectionStatusEventArgs
    {
        public ConnectionStatuses Status { get; set; }
    }

    public class ConnectionErrorEventArgs
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}

using CrossfireCore.Utility;
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

        const int DefaultServerPort = 13327;
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

        public bool Connect(string Host = DefaultServerHost, int Port = DefaultServerPort)
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
                        WaitForHeader(new StateObject(client, _stream));

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
            SetConnectionStatus(ConnectionStatuses.Connecting);

            return true;
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            var client = ar.AsyncState as TcpClient;

            System.Diagnostics.Debug.Assert(client != null);
            System.Diagnostics.Debug.Assert(client == _client);

            try
            {
                client.EndConnect(ar);
            }
            catch (SocketException ex)
            {
                OnError?.Invoke(this, new ConnectionErrorEventArgs()
                {
                    ErrorCode = ex.ErrorCode,
                    ErrorMessage = ex.Message
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
            WaitForHeader(new StateObject(client, _stream));

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
        }

        public void Disconnect()
        {
            if ((_client != null) && (_client.Connected))
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

            _stream.Write(lengthBytes, 0, lengthBytes.Length);
            _stream.Write(Message, 0, Message.Length);

            _Logger.Debug("Write {0} bytes\n{1}", 
                messageLength, HexDump.Utils.HexDump(Message));

            return true;
        }

        const int HeaderSize = 2;

        private void WaitForHeader(StateObject so)
        {
            System.Diagnostics.Debug.Assert(so.client == _client);

            //stream has been shut down
            //TODO: cleanup/disconnect here?
            if (!so.stream.CanRead)
                return;

            //setup state
            so.waitingForHeader = true;
            so.wantLen = HeaderSize;
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

        private void WaitForMessage(StateObject so, int MessageLength)
        {
            System.Diagnostics.Debug.Assert(so.client == _client);

            //stream has been shut down
            //TODO: cleanup/disconnect here?
            if (!so.stream.CanRead)
                return;

            //setup state
            so.waitingForHeader = false;
            so.wantLen = MessageLength;
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
            if (so.bufferLen < so.wantLen)
            {
                _Logger.Debug("Partial Packet: Read:{0} Want:{1} Have:{2}",
                    bytesRead, so.wantLen, so.bufferLen);

                try
                {
                    var asyncResult = so.stream.BeginRead(so.buffer, so.bufferLen, so.wantLen - so.bufferLen, BeginReadCallback, so);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new ConnectionErrorEventArgs()
                    {
                        ErrorMessage = ex.Message
                    });

                    Disconnect();
                }
                return;
            }

            //We have received the amount of data we have expected: 
            //If waiting for length, then parse it and wait for that amount of data
            //Otherwise, we have a message from the server to handle
            if (so.waitingForHeader)
            {
                var messageLen = (so.buffer[0] * 256) + so.buffer[1];
                
                //var messageLen = BitConverter.ToUInt16(new byte[] { so.buffer[1], so.buffer[0] }, 0);
                //TODO: ...
                //BitConverter.IsLittleEndian
                //messageLen = System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToUInt16(so.buffer, 0));


                if (messageLen > StateObject.BufferSize)
                    throw new Exception("Internal Data Mismatch");

                WaitForMessage(so, messageLen);
            }
            else
            {
                var args = new ConnectionPacketEventArgs();
                args.Packet = new byte[so.wantLen];
                Array.Copy(so.buffer, args.Packet, so.wantLen);

                OnPacket?.Invoke(this, args);

                WaitForHeader(so);
            }
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

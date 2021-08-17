using Crossfire.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.ServerInterface
{
    public class Connection
    {
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

        public ConnectionStatuses ConnectionStatus { get; private set; }

        public bool Connect(string Host = "127.0.0.1", int Port = 13327)
        {
            Disconnect();

            _client = new TcpClient
            {
                NoDelay = true
            };

            try
            {
                var asyncResult = _client.BeginConnect(Host, Port, ConnectCallback, _client);

                if (asyncResult.CompletedSynchronously)
                    throw new Exception();
                if (asyncResult.IsCompleted)
                    throw new Exception();
            }
            catch (SocketException ex)
            {
                OnError?.Invoke(this, new ConnectionErrorEventArgs()
                {
                    ErrorMessage = ex.Message
                });

                Cleanup();
                return false;
            }
            catch (Exception ex)
            {
                Cleanup();
                return false;
            }

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

                Cleanup();
                return;
            }
            catch (Exception ex)
            {
                Cleanup();
                return;
            }

            if (!client.Connected)
            {
                Cleanup();
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
                SetConnectionStatus(ConnectionStatuses.Disconnected);
            }

            Cleanup();
        }


        public bool SendMessage(string Message)
        {
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

            Logger.Log(Logger.Levels.Debug, "C->S: len={0}", messageLength);
            Logger.Log(Logger.Levels.Comm, "{0}", HexDump.Utils.HexDump(Message));

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

            var asyncResult = so.stream.BeginRead(so.buffer, 0, so.wantLen, BeginReadCallback, so);
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

            var asyncResult = so.stream.BeginRead(so.buffer, 0, so.wantLen, BeginReadCallback, so);
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
                Disconnect();
                return;
            }

            if (bytesRead == 0)
            {
                Disconnect();
                return;
            }

            //collect buffer data
            so.bufferLen += bytesRead;
            if (so.bufferLen < so.wantLen)
            {
                Logger.Log(Logger.Levels.Debug, "Partial Packet: Read:{0} Want:{1} Have:{2}",
                    bytesRead, so.wantLen, so.bufferLen);

                var asyncResult = so.stream.BeginRead(so.buffer, so.bufferLen, so.wantLen - so.bufferLen, BeginReadCallback, so);
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
            ConnectionStatus = Status;

            OnStatusChanged?.Invoke(this, new ConnectionStatusEventArgs() 
            {
                Status = Status,
            });
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

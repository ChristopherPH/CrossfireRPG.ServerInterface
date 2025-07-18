﻿/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using Common.Logging;
using CrossfireRPG.ServerInterface.Definitions;
using System;
using System.Net.Sockets;
using System.Text;


namespace CrossfireRPG.ServerInterface.Network
{
    public class SocketConnection
    {
        public static Logger Logger { get; } = new Logger(nameof(SocketConnection));

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

        public bool Connect(string Host = DefaultServerHost, int Port = Config.CSPORT,
            AddressFamily family = AddressFamily.Unspecified)
        {
            Disconnect();

            if (family == AddressFamily.Unspecified)
                family = NetworkUtility.CheckHostIPv6(Host) ?
                    AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;

            _client = new TcpClient(family)
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
                RaiseOnError(ex.ErrorCode, ex.Message);

                //at this point, we didn't set our connection status,
                //and never got a valid socket, so just clean up
                //instead of disconnecting
                Cleanup();
                return false;
            }
            catch (Exception ex)
            {
                RaiseOnError(ex.Message);

                Cleanup();
                return false;
            }

            this.Host = Host;
            this.Port = Port;

            Logger.Info($"Connecting to {this.Host}:{this.Port}");

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

                RaiseOnError(ex.ErrorCode, msg);

                Disconnect();
                return;
            }
            catch (Exception ex)
            {
                RaiseOnError(ex.Message);

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

            //notify connected
            Logger.Info($"Connected to {this.Host}:{this.Port}");

            SetConnectionStatus(ConnectionStatuses.Connected);

            //start waiting for data
            WaitForBytes(new StateObject(client, _stream));
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

                Logger.Info($"Disconnected from {this.Host}:{this.Port}");
            }
            else
            {
                Logger.Info($"Disconnected from {this.Host}:{this.Port} (but wasn't connected)");
            }

            SetConnectionStatus(ConnectionStatuses.Disconnected);

            Cleanup();
        }


        public bool SendMessage(string Message)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(Message));

            var messageBytes = Encoding.UTF8.GetBytes(Message);

            return SendMessage(messageBytes);
        }

        public bool SendMessage(string Format, params object[] args)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(Format));

            string Message;

            try
            {
                Message = string.Format(Format, args);
            }
            catch (ArgumentNullException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }

            System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(Message));

            var messageBytes = Encoding.UTF8.GetBytes(Message);

            return SendMessage(messageBytes);
        }

        public bool SendMessage(byte[] Message)
        {
            if ((Message == null) ||
                (Message.Length == 0) ||
                (Message.Length > 65535) ||
                (ConnectionStatus != ConnectionStatuses.Connected))
            {
                return false;
            }

            if ((_client == null) || (_stream == null) || (!_client.Connected))
            {
                Disconnect();
                return false;
            }

            if (!_stream.CanWrite)
            {
                Disconnect();
                return false;
            }

            //Message length must fit within 2 bytes
            var messageLength = Message.Length;

            //Create a single send buffer
            var sendBytes = new byte[2 + messageLength];

            //Add message length to send buffer
            sendBytes[0] = (byte)((messageLength >> 8) & 0xFF);
            sendBytes[1] = (byte)((messageLength) & 0xFF);

            //Add message to send buffer
            Array.Copy(Message, 0, sendBytes, 2, messageLength);

            //Send message
            try
            {
                _stream.BeginWrite(sendBytes, 0, sendBytes.Length, BeginSendCallback, _stream);
            }
            catch (System.IO.IOException ex)
            {
                var msg = ex.Message;

                if (ex.InnerException is SocketException se)
                {
                    switch (se.ErrorCode)
                    {
                        //Don't send a ConnectionErrorEventArgs on these error codes
                        case 10054: //WSAECONNRESET / Connection reset by peer.
                            Logger.Info($"Connection reset by peer while sending message");
                            break;

                        //use socket error message instead of System.IO.IOException error message
                        default:
                            RaiseOnError(se.ErrorCode, se.Message);
                            break;
                    }
                }
                else
                {
                    RaiseOnError(ex.Message);
                }

                Disconnect();
                return false;
            }

            Logger.Debug("Write {0} bytes\n{1}",
                messageLength, HexDump.Utils.HexDump(Message));

            return true;
        }

        private void BeginSendCallback(IAsyncResult ar)
        {
            var stream = ar.AsyncState as NetworkStream;

            //If we have called disconnect() due to a write failure,
            //and there are other pending writes, the client/stream
            //will not match the state objects client/stream,
            //so we can ignore this BeginSendCallback()
            if ((_client == null) || (_stream == null))
                return;

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
                if (ex.InnerException is SocketException se)
                {
                    switch (se.ErrorCode)
                    {
                        //Don't send a ConnectionErrorEventArgs on these error codes
                        case 10054: //WSAECONNRESET / Connection reset by peer.
                            Logger.Info($"Connection reset by peer while sending message callback");
                            break;

                        //use socket error message instead of System.IO.IOException error message
                        default:
                            RaiseOnError(se.ErrorCode, se.Message);
                            break;
                    }
                }
                else
                {
                    RaiseOnError(ex.Message);
                }

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
                if (ex.InnerException is SocketException se)
                {
                    switch (se.ErrorCode)
                    {
                        //Don't send a ConnectionErrorEventArgs on these error codes
                        case 10054: //WSAECONNRESET / Connection reset by peer.
                            Logger.Info($"Connection reset by peer while reading message");
                            break;

                        //use socket error message instead of System.IO.IOException error message
                        default:
                            RaiseOnError(se.ErrorCode, se.Message);
                            break;
                    }
                }
                else
                {
                    RaiseOnError(ex.Message);
                }

                Disconnect();
            }
        }

        private void BeginReadCallback(IAsyncResult ar)
        {
            var so = ar.AsyncState as StateObject;

            System.Diagnostics.Debug.Assert(so != null);

            //If we have called disconnect() due to a write failure,
            //then this read is still active but the client/stream
            //will not match the state objects client/stream,
            //so we can ignore this BeginReadCallback()
            if ((_client == null) || (_stream == null))
                return;

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
                            Logger.Info($"Connection reset by peer while reading message callback");
                            break;

                        //use socket error message instead of System.IO.IOException error message
                        default:
                            RaiseOnError(se.ErrorCode, se.Message);
                            break;
                    }
                }
                else
                {
                    RaiseOnError(ex.Message);
                }

                Disconnect();
                return;
            }
            catch (Exception ex)
            {
                RaiseOnError(ex.Message);

                Disconnect();
                return;
            }

            if (bytesRead == 0)
            {
                Disconnect();
                return;
            }

            Logger.Debug("Read {0} bytes\n{1}", bytesRead,
                HexDump.Utils.HexDump(so.buffer, so.bufferLen, bytesRead));

            //collect buffer data
            so.bufferLen += bytesRead;

            var tmpBuffer = new byte[bytesRead];
            Array.Copy(so.buffer, tmpBuffer, bytesRead);

            RaiseOnPacket(tmpBuffer);

            WaitForBytes(so);
        }


        public event EventHandler<ConnectionStatusEventArgs> OnStatusChanged;
        public event EventHandler<ConnectionPacketEventArgs> OnPacket;
        public event EventHandler<ConnectionErrorEventArgs> OnError;

        protected void RaiseOnStatusChanged(ConnectionStatuses Status)
        {
            OnStatusChanged?.Invoke(this, new ConnectionStatusEventArgs()
            {
                Status = Status
            });
        }

        protected virtual void RaiseOnPacket(byte[] Packet)
        {
            OnPacket?.Invoke(this, new ConnectionPacketEventArgs()
            {
                Packet = Packet
            });
        }

        protected virtual void RaiseOnError(int ErrorCode, string ErrorMessage)
        {
            OnError?.Invoke(this, new ConnectionErrorEventArgs()
            {
                ErrorCode = ErrorCode,
                ErrorMessage = ErrorMessage
            });
        }

        protected void RaiseOnError(string ErrorMessage)
        {
            RaiseOnError(0, ErrorMessage);
        }

        protected virtual void SetConnectionStatus(ConnectionStatuses Status)
        {
            if (ConnectionStatus != Status)
            {
                ConnectionStatus = Status;
                RaiseOnStatusChanged(Status);
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


    public class ConnectionPacketEventArgs : EventArgs
    {
        public byte[] Packet { get; set; }
    }

    public class ConnectionStatusEventArgs : EventArgs
    {
        public ConnectionStatuses Status { get; set; }
    }

    public class ConnectionErrorEventArgs: EventArgs
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}

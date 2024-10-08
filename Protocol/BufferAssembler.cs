﻿/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System;
using System.IO;
using System.Text;

namespace CrossfireRPG.ServerInterface.Protocol
{
    public class BufferAssembler : IDisposable
    {
        public string Command { get; }
        public long DataLength => commandBuffer.Length - Command.Length;

        MemoryStream commandBuffer = new MemoryStream();
        private bool disposedValue;

        public BufferAssembler(string Command, bool AddSpaceAfterCommand = true)
        {
            if (string.IsNullOrWhiteSpace(Command))
                throw new ArgumentException("Invalid Parameter", nameof(Command));

            this.Command = Command;
            var commandBytes = Encoding.UTF8.GetBytes(Command);
            commandBuffer.Write(commandBytes, 0, commandBytes.Length);

            if (AddSpaceAfterCommand)
                AddSpace();
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Command, commandBuffer.Length);
        }

        void AddBytes(byte[] bytes)
        {
            if ((bytes == null) || (bytes.Length == 0))
                throw new ArgumentException("Invalid Parameter", nameof(bytes));

            commandBuffer.Write(bytes, 0, bytes.Length);
        }

        public void AddByte(byte b)
        {
            commandBuffer.Write(new byte[] { b }, 0, 1);
        }

        public void AddUInt16(UInt16 i)
        {
            var bytes = new byte[2];

            bytes[0] = (byte)((i >> 8) & 0xFF);
            bytes[1] = (byte)((i) & 0xFF);

            commandBuffer.Write(bytes, 0, bytes.Length);
        }

        public void AddUInt32(UInt32 i)
        {
            var bytes = new byte[4];

            bytes[0] = (byte)((i >> 24) & 0xFF);
            bytes[1] = (byte)((i >> 16) & 0xFF);
            bytes[2] = (byte)((i >> 8) & 0xFF);
            bytes[3] = (byte)((i) & 0xFF);

            commandBuffer.Write(bytes, 0, bytes.Length);
        }

        public void AddInt32(Int32 i)
        {
            var bytes = new byte[4];

            bytes[0] = (byte)((i >> 24) & 0xFF);
            bytes[1] = (byte)((i >> 16) & 0xFF);
            bytes[2] = (byte)((i >> 8) & 0xFF);
            bytes[3] = (byte)((i) & 0xFF);

            commandBuffer.Write(bytes, 0, bytes.Length);
        }

        public void AddIntAsString(Int32 i)
        {
            AddString(i.ToString());
        }

        public void AddIntAsString(UInt32 i)
        {
            AddString(i.ToString());
        }

        public void AddString(string s)
        {
            //Do not allow empty strings
            if (string.IsNullOrEmpty(s))
                throw new ArgumentException("Invalid Parameter", nameof(s));

            var bytes = Encoding.UTF8.GetBytes(s);
            commandBuffer.Write(bytes, 0, bytes.Length);
        }

        public void AddString(string format, params object[] args)
        {
            AddString(string.Format(format, args));
        }

        public void AddLengthPrefixedString(string s)
        {
            //Allow a 0 byte length prefixed string
            if (s == null)
                throw new ArgumentException("Invalid Parameter", nameof(s));

            var bytes = Encoding.UTF8.GetBytes(s);

            if (bytes.Length > byte.MaxValue)
                throw new ArgumentException("Invalid Parameter", nameof(s));

            AddByte((byte)bytes.Length);

            commandBuffer.Write(bytes, 0, bytes.Length);
        }

        public void AddLengthPrefixedString(string format, params object[] args)
        {
            AddLengthPrefixedString(string.Format(format, args));
        }

        public void AddSpace()
        {
            AddByte((byte)' ');
        }

        public byte[] GetBytes()
        {
            return commandBuffer.ToArray();
        }
        
        /*
        public bool SendBuffer(SocketConnection connection)
        {
            return connection.SendMessage(GetBytes());
        }
        */

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

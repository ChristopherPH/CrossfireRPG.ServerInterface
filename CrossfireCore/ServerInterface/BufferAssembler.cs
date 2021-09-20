using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossfireCore.ServerInterface
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
            var commandBytes = Encoding.ASCII.GetBytes(Command);
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

        //TODO: verify 
        public void AddInt32(Int32 i)
        {
            var bytes = new byte[4];

            bytes[0] = (byte)((i >> 24) & 0xFF);
            bytes[1] = (byte)((i >> 16) & 0xFF);
            bytes[2] = (byte)((i >> 8) & 0xFF);
            bytes[3] = (byte)((i) & 0xFF);

            commandBuffer.Write(bytes, 0, bytes.Length);
        }

        public void AddIntAsString(int i)
        {
            AddString(i.ToString());
        }

        public void AddIntAsString(UInt32 i)
        {
            AddString(i.ToString());
        }

        public void AddString(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentException("Invalid Parameter", nameof(s));

            var bytes = Encoding.ASCII.GetBytes(s);
            commandBuffer.Write(bytes, 0, bytes.Length);
        }

        public void AddString(string format, params object[] args)
        {
            AddString(string.Format(format, args));
        }

        public void AddLengthPrefixedString(string s)
        {
            if (string.IsNullOrEmpty(s) || (s.Length > byte.MaxValue))
                throw new ArgumentException("Invalid Parameter", nameof(s));

            AddByte((byte)s.Length);

            var bytes = Encoding.ASCII.GetBytes(s);
            commandBuffer.Write(bytes, 0, bytes.Length);
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

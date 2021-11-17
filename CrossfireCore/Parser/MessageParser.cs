using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParser : MessageParserBase
    {
        public event EventHandler<ParseBufferEventArgs> BeginParseBuffer;
        public event EventHandler<ParseBufferEventArgs> EndParseBuffer;

        public MessageParser(SocketConnection Connection)
            : base(Connection) { }

        protected override void ParseBuffer(ref byte[] SavedBuffer, byte[] Buffer,
            out int ByteCount, out int MessageCount)
        {
            var args = new ParseBufferEventArgs()
            {
                SavedBuffer = SavedBuffer,
                Buffer = Buffer,
            };

            BeginParseBuffer?.Invoke(this, args);

            base.ParseBuffer(ref SavedBuffer, Buffer, out ByteCount, out MessageCount);

            args.ByteCount = ByteCount;
            args.MessageCount = MessageCount;

            EndParseBuffer?.Invoke(this, args);
        }

        public class ParseBufferEventArgs : SingleCommandEventArgs
        {
            public byte[] SavedBuffer { get; set; } = null;
            public byte[] Buffer { get; set; } = null;
            public int ByteCount { get; set; } = 0;
            public int MessageCount { get; set; } = 0;

            public byte[] TotalBuffer
            {
                get
                {
                    if (SavedBuffer == null)
                        return Buffer;

                    if (Buffer == null)
                        return SavedBuffer;

                    var tmpBuffer = new byte[SavedBuffer.Length + Buffer.Length];

                    Array.Copy(SavedBuffer, 0, tmpBuffer, 0, SavedBuffer.Length);
                    Array.Copy(Buffer, 0, tmpBuffer, SavedBuffer.Length, Buffer.Length);

                    return tmpBuffer;
                }
            }
        }
    }
}

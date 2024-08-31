using CrossfireRPG.ServerInterface.Network;
using System;

namespace CrossfireCore.ServerInterface
{
    /// <summary>
    /// MessageParser is used to receive messages from the server and generate
    /// events that other objects can subscribe to
    /// </summary>
    public partial class MessageHandler : MessageParser
    {
        public event EventHandler<ParseBufferEventArgs> BeginParseBuffer;
        public event EventHandler<ParseBufferEventArgs> EndParseBuffer;

        public MessageHandler(SocketConnection Connection)
            : base(Connection) { }

        /// <summary>
        /// Overrides parsing buffer to send a begin and end event wrapped around the parsing.
        /// Note: The server often sends commands batched (multiple commands arrive in a single
        /// packet if the network latency is low. This means we can try to minimize updates if
        /// we know that there are multiple similar commands (eg: inventory add) as part of
        /// a single batch). See the data managers as an example.
        /// </summary>
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

        public class ParseBufferEventArgs : EventArgs
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

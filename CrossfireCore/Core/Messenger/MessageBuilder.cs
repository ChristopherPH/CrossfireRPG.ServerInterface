﻿using Common;

namespace CrossfireCore.ServerInterface
{
    /// <summary>
    /// MessageBuilder is used to create and send messages to the server
    /// Note: This is a partial class and commands are implemented across multiple files
    /// </summary>
    public partial class MessageBuilder
    {
        public static Logger Logger { get; } = new Logger(nameof(MessageBuilder));

        /// <summary>
        /// CrossfireCore.ServerInterface can receive and understand protocol
        /// messages up to and including version:
        /// </summary>
        public const int ClientProtocolVersion = 1023;

        public MessageBuilder(SocketConnection Connection)
        {
            this._Connection = Connection;
        }

        private SocketConnection _Connection;

        /// <summary>
        /// Sends an assembled message to the server
        /// </summary>
        /// <returns>True if the message was sent, false if not</returns>
        private bool SendMessage(BufferAssembler ba)
        {
            if (ba == null)
                return false;

            Logger.Info("C->S: cmd={0}, datalen={1}", ba.Command, ba.DataLength);

            var bytes = ba.GetBytes();
            Logger.Debug("\n{0}", HexDump.Utils.HexDump(bytes));

            return _Connection.SendMessage(bytes);
        }

        /// <summary>
        /// Sends a raw string to the server
        /// </summary>
        /// <returns>True if the message was sent, false if not</returns>
        public bool SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return false;

            message = message.Trim();

            Logger.Info("C->S: message={0}", message);

            return _Connection.SendMessage(message);
        }
    }
}

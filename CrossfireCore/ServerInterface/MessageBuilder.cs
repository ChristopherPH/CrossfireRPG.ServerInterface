using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageBuilder
    {
        static Logger _Logger = new Logger(nameof(MessageBuilder));

        /// <summary>
        /// CrossfireCore.ServerInterface can receive and understand protocol messages up to and including version:
        /// </summary>
        public const int ClientProtocolVersion = 1023;

        public MessageBuilder(SocketConnection Connection)
        {
            this._Connection = Connection;
        }

        private SocketConnection _Connection;
        private UInt16 nComPacket = 1;

        private bool SendMessage(BufferAssembler ba)
        {
            _Logger.Info("C->S: cmd={0}, datalen={1}", ba.Command, ba.DataLength);

            var bytes = ba.GetBytes();
            _Logger.Debug("\n{0}", HexDump.Utils.HexDump(bytes));

            return _Connection.SendMessage(bytes);
        }
    }
}

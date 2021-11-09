using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossfireCore.ServerInterface
{
    public abstract partial class MessageParserBase
    {
        static Logger _Logger = new Logger(nameof(MessageParserBase));

        public const int ServerProtocolVersion = 1039;

        public MessageParserBase(SocketConnection Connection)
        {
            _Connection = Connection;
            _Connection.OnPacket += ParsePacket;

            AddAccountParsers();
            AddAudioParsers();
            AddCommandParsers();
            AddGraphicsParsers();
            AddItemParsers();
            AddMapParsers();
            AddMiscParsers();
            AddPlayerParsers();
            AddProtocolParsers();
            AddQuestParsers();
            AddSpellParsers();
        }

        private SocketConnection _Connection;
        private Dictionary<string, ParseCommand> _CommandHandler = new Dictionary<string, ParseCommand>();

        protected bool AddCommandHandler(string command, ParseCommand parseCommand)
        {
            if (string.IsNullOrWhiteSpace(command) || (parseCommand == null) || (parseCommand.Parser == null))
                return false;

            if (_CommandHandler.ContainsKey(command))
                return false;

            _CommandHandler[command] = parseCommand;
            return true;
        }

        protected virtual void ParsePacket(object sender, ConnectionPacketEventArgs e)
        {
            System.Diagnostics.Debug.Assert(e != null);
            System.Diagnostics.Debug.Assert(e.Packet != null);
            System.Diagnostics.Debug.Assert(e.Packet.Length > 0);

            int offset = 0;
            var cmd = BufferTokenizer.GetString(e.Packet, ref offset);

            System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(cmd));

            //run command parser
            if (_CommandHandler.TryGetValue(cmd, out var parseCommand))
            {
                _Logger.Log(parseCommand.Level, "S->C: cmd={0}, datalen={1}", cmd, e.Packet.Length - offset);

                try
                {
                    if (!parseCommand.Parser(e.Packet, ref offset))
                        _Logger.Error("Failed to parse command: {0}", cmd);
                }
                catch (Exception ex)
                {
                    _Logger.Error("Failed to parse command: {0}: {1}",
                        cmd, ex.Message);
                }
            }
            else
            {
                _Logger.Warning("Unhandled Command: {0}", cmd);
            }

            //log excess data
            if (offset < e.Packet.Length)
            {
                _Logger.Warning("Excess Data for cmd {0}:\n{1}",
                    cmd, HexDump.Utils.HexDump(e.Packet, offset));
            }
        }

        public class ParseCommand
        {
            public delegate bool ParseCommandCallback(byte[] packet, ref int offset);

            public ParseCommand() { }

            public ParseCommand(ParseCommandCallback Parser)
            {
                this.Parser = Parser;
            }

            public ParseCommand(ParseCommandCallback Parser, Logger.Levels Level)
            {
                this.Parser = Parser;
                this.Level = Level;
            }

            public ParseCommandCallback Parser { get; set; }
            public Logger.Levels Level { get; set; } = Logger.Levels.Info;
        }
    }
}

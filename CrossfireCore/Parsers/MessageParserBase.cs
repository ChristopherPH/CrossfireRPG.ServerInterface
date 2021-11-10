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
            _Connection.OnPacket += ParseBuffer;

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

        byte[] _SavedPacket = null;

        protected virtual void ParseBuffer(object sender, ConnectionPacketEventArgs e)
        {
            byte[] workingPacket;

            if (_SavedPacket == null)
            {
                workingPacket = e.Packet;
            }
            else //combine buffers
            {
                workingPacket = new byte[_SavedPacket.Length + e.Packet.Length];
                Array.Copy(_SavedPacket, 0, workingPacket, 0,  _SavedPacket.Length);
                Array.Copy(e.Packet, 0, workingPacket, _SavedPacket.Length, e.Packet.Length);

                _SavedPacket = null;
            }

            const int MessageLengthSize = 2;
            int offset = 0;

            if (workingPacket.Length <= MessageLengthSize)
            {
                _SavedPacket = workingPacket;
                return;
            }

            //get messages out of packet
            while (offset < workingPacket.Length)
            {
                //ensure enough data to retrieve message size
                if (offset + MessageLengthSize > workingPacket.Length)
                {
                    //save any extra bytes for next call to ParseBuffer
                    if (offset < workingPacket.Length)
                    {
                        _SavedPacket = new byte[workingPacket.Length - offset];
                        Array.Copy(workingPacket, offset, _SavedPacket, 0, _SavedPacket.Length);
                    }

                    break;
                }

                //read message length
                var messageLen = (workingPacket[offset] * 256) + workingPacket[offset + 1];

                if (messageLen <= 0)
                    throw new Exception("Invalid message length");

                //ensure enough data to retrieve message
                if (offset + MessageLengthSize + messageLen > workingPacket.Length)
                {
                    //save any extra bytes, including the message length, for next call to ParseBuffer
                    if (offset < workingPacket.Length)
                    {
                        _SavedPacket = new byte[workingPacket.Length - offset];
                        Array.Copy(workingPacket, offset, _SavedPacket, 0, _SavedPacket.Length);
                    }

                    break;
                }

                //point to start of message
                offset += MessageLengthSize;

                //process message
                ParsePacket(workingPacket, offset, messageLen);

                //point to start of next message
                offset += messageLen;
            }
        }

        protected virtual void ParsePacket(byte[] Packet, int DataOffset, int DataLength)
        {
            System.Diagnostics.Debug.Assert(Packet != null);
            System.Diagnostics.Debug.Assert(Packet.Length > 0);

            var offset = DataOffset;
            var end = DataOffset + DataLength;
            var cmd = BufferTokenizer.GetString(Packet, ref offset, end);

            System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(cmd));

            //run command parser
            if (_CommandHandler.TryGetValue(cmd, out var parseCommand))
            {
                _Logger.Log(parseCommand.Level, "S->C: cmd={0}, datalen={1}", cmd, DataLength);

                try
                {
                    if (!parseCommand.Parser(Packet, ref offset, end))
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
            if (offset < end)
            {
                _Logger.Warning("Excess Data for cmd {0}:\n{1}",
                    cmd, HexDump.Utils.HexDump(Packet, offset, end - offset));
            }
            else if (offset > end)
            {
                _Logger.Warning("Used too much data for cmd {0}",
                    cmd, HexDump.Utils.HexDump(Packet, offset - end));
            }
        }

        public class ParseCommand
        {
            public delegate bool ParseCommandCallback(byte[] packet, ref int offset, int end);

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

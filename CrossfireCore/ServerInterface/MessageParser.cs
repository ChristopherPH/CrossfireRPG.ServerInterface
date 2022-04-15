using Common;
using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossfireCore.ServerInterface
{
    public abstract partial class MessageParser
    {
        static Logger _Logger = new Logger(nameof(MessageParser));

        public const int ServerProtocolVersion = 1039;

        public MessageParser(SocketConnection Connection)
        {
            _Connection = Connection;
            _Connection.OnPacket += Connection_OnPacket;

            AddAccountParsers();
            AddAudioParsers();
            AddCommandParsers();
            AddGraphicsParsers();
            AddInfoParsers();
            AddItemParsers();
            AddKnowledgeParsers();
            AddMapParsers();
            AddMessageParsers();
            AddMiscParsers();
            AddPlayerParsers();
            AddProtocolParsers();
            AddQuestParsers();
            AddSpellParsers();
        }

        private SocketConnection _Connection;
        private Dictionary<string, CommandParserDefinition> _CommandHandler = new Dictionary<string, CommandParserDefinition>();

        protected bool AddCommandHandler(string command, CommandParserDefinition parseCommand)
        {
            if (string.IsNullOrWhiteSpace(command) || (parseCommand == null) || (parseCommand.Parser == null))
                return false;

            if (_CommandHandler.ContainsKey(command))
                return false;

            _CommandHandler[command] = parseCommand;
            return true;
        }

        byte[] _SavedBuffer = null;

        private void Connection_OnPacket(object sender, ConnectionPacketEventArgs e)
        {
            ParseBuffer(ref _SavedBuffer, e.Packet, out var ByteCount, out var MessageCount);
        }

        protected virtual void ParseBuffer(ref byte[] SavedBuffer, byte[] Buffer,
            out int ByteCount, out int MessageCount)
        {
            byte[] workingBuffer;

            if (SavedBuffer == null)
            {
                //no saved buffer, we can just use the passed in buffer as a working buffer
                workingBuffer = Buffer;
            }
            else //combine saved buffer with new buffer
            {
                workingBuffer = new byte[SavedBuffer.Length + Buffer.Length];

                Array.Copy(SavedBuffer, 0, workingBuffer, 0,  SavedBuffer.Length);
                Array.Copy(Buffer, 0, workingBuffer, SavedBuffer.Length, Buffer.Length);

                SavedBuffer = null;
            }

            const int MessageLengthSize = 2;
            ByteCount = 0;
            MessageCount = 0;

            //get messages out of packet
            while (ByteCount < workingBuffer.Length)
            {
                //ensure enough data to retrieve message size
                if (ByteCount + MessageLengthSize > workingBuffer.Length)
                {
                    //save any extra bytes for next call to ParseBuffer
                    if (ByteCount < workingBuffer.Length)
                    {
                        SavedBuffer = new byte[workingBuffer.Length - ByteCount];
                        Array.Copy(workingBuffer, ByteCount, SavedBuffer, 0, SavedBuffer.Length);
                    }

                    break;
                }

                //read message length
                var messageLen = (workingBuffer[ByteCount] * 256) + workingBuffer[ByteCount + 1];

                if (messageLen <= 0)
                    throw new Exception("Invalid message length");

                //ensure enough data to retrieve message
                if (ByteCount + MessageLengthSize + messageLen > workingBuffer.Length)
                {
                    //save any extra bytes, including the message length, for next call to ParseBuffer
                    if (ByteCount < workingBuffer.Length)
                    {
                        SavedBuffer = new byte[workingBuffer.Length - ByteCount];
                        Array.Copy(workingBuffer, ByteCount, SavedBuffer, 0, SavedBuffer.Length);
                    }

                    break;
                }

                //point to start of message
                ByteCount += MessageLengthSize;

                //process message
                ParseMessage(workingBuffer, ByteCount, messageLen);

                //point to start of next message
                ByteCount += messageLen;

                MessageCount++;
            }
        }

        protected virtual void ParseMessage(byte[] Message, int DataOffset, int DataLength)
        {
            System.Diagnostics.Debug.Assert(Message != null);
            System.Diagnostics.Debug.Assert(Message.Length > 0);

            var curPos = DataOffset;
            var dataEnd = DataOffset + DataLength;
            var command = BufferTokenizer.GetString(Message, ref curPos, dataEnd);

            System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(command));

            //run command parser
            if (_CommandHandler.TryGetValue(command, out var parseCommand))
            {
                _Logger.Log(parseCommand.Level, "S->C: command={0}, datalength={1}", command, DataLength);
                _Logger.Debug("{0}", HexDump.Utils.HexDump(Message, curPos, dataEnd - curPos));

                try
                {
                    if (!parseCommand.Parser(Message, ref curPos, dataEnd))
                        _Logger.Error("Failed to parse command: {0}", command);
                }
                catch (Exception ex)
                {
                    _Logger.Error("Failed to parse command: {0}: {1}",
                        command, ex.Message);
                }
            }
            else
            {
                _Logger.Warning("Unhandled Command: {0}", command);
                _Logger.Info("{0}", HexDump.Utils.HexDump(Message, curPos, dataEnd - curPos));
                return;
            }

            //log excess data
            if (curPos < dataEnd)
            {
                _Logger.Warning("Excess data for command {0}: {1} bytes",
                    command, dataEnd - curPos);
                _Logger.Info("{0}", HexDump.Utils.HexDump(Message, curPos, dataEnd - curPos));
            }
            else if (curPos > dataEnd)
            {
                _Logger.Warning("Used too much data for command {0}: {1}",
                    command, curPos - dataEnd);
            }
        }
    }
}

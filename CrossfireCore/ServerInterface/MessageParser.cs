using Common;
using System;
using System.Collections.Generic;

namespace CrossfireCore.ServerInterface
{
    /// <summary>
    /// MessageParser is used to receive and parse messages from the server
    /// Abstract functions are used so derived classes can handle the message in different ways
    /// Note: This is a partial class and commands are implemented across multiple files
    /// </summary>
    public abstract partial class MessageParser
    {
        static Logger _Logger = new Logger(nameof(MessageParser));

        /// <summary>
        /// CrossfireCore.ServerInterface can send protocol messages up to and including version:
        /// </summary>
        public const int ServerProtocolMaximumVersion = 1029;

        //Save the protocol version for the parser
        public int ServerProtocolVersion { get; private set; } = 0;

        const float FLOAT_MULTF = 1000.0f;

        public MessageParser(SocketConnection Connection)
        {
            _Connection = Connection;
            _Connection.OnPacket += Connection_OnPacket;
            _Connection.OnStatusChanged += _Connection_OnStatusChanged;

            //Add all the different parsers to the command handler
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

        /// <summary>
        /// Add a command handler and parser function for the command data
        /// </summary>
        /// <returns>true if the handler was added</returns>
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

        /// <summary>
        /// Parses messages from the incoming buffer and the current saved buffer
        /// </summary>
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
                    throw new MessageParserException("Message length <= 0");

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
                if (dataEnd - curPos > 0) //if there is data after the command
                    _Logger.Debug("\n{0}", HexDump.Utils.HexDump(Message, curPos, dataEnd - curPos));

                try
                {
                    if (!parseCommand.Parser(Message, ref curPos, dataEnd))
                        _Logger.Error("Failed to parse command: {0}", command);
                }
                catch (BufferTokenizerException ex)
                {
                    _Logger.Error("Failed to tokenize buffer: {0}: {1}",
                        command, ex.Message);
                }
                catch (MessageParserException ex)
                {
                    _Logger.Error("Failed to parse command: {0}: {1}",
                        command, ex.Message);
                }
            }
            else
            {
                _Logger.Warning("Unhandled Command: {0}", command);
                _Logger.Info("\n{0}", HexDump.Utils.HexDump(Message, curPos, dataEnd - curPos));
                return;
            }

            //log excess data
            if (curPos < dataEnd)
            {
                _Logger.Warning("Excess data for command {0}: {1} bytes",
                    command, dataEnd - curPos);
                _Logger.Info("\n{0}", HexDump.Utils.HexDump(Message, curPos, dataEnd - curPos));
            }
            else if (curPos > dataEnd)
            {
                _Logger.Warning("Used too much data for command {0}: {1}",
                    command, curPos - dataEnd);
            }
        }

        private void _Connection_OnStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            //Only clean up on a disconnect. If we clean up on any status change (connect)
            //then we somtimes clear items after they've been set (event race condition)
            if (e.Status == ConnectionStatuses.Disconnected)
            {
                _SavedBuffer = null;

                //Reset the parser options
                ParserOption_SpellMon = 0;
                ServerProtocolVersion = 0;
            }
        }
    }


    [Serializable]
    public class MessageParserException : Exception

    {
        public MessageParserException() { }
        public MessageParserException(string message) : base(message) { }
        public MessageParserException(string message, Exception inner) : base(message, inner) { }
        protected MessageParserException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

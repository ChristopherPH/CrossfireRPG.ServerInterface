using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.Parser
{
    public partial class MessageParser
    {
        protected abstract void HandleCompletedCommand(UInt16 comc_packet, UInt32 comc_time);
        protected abstract void HandleQuery(int Flags, string QueryText);

        private void AddCommandParsers()
        {
            AddCommandHandler("comc", new CommandParserDefinition(Parse_comc));
            AddCommandHandler("query", new CommandParserDefinition(Parse_query));
        }

        private bool Parse_comc(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var comc_packet = BufferTokenizer.GetUInt16(Message, ref DataOffset);
            var comc_time = BufferTokenizer.GetUInt32(Message, ref DataOffset);
            //TODO: convert completed command time as it isn't quite correct

            HandleCompletedCommand(comc_packet, comc_time);

            return true;
        }

        private bool Parse_query(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var query_flags = BufferTokenizer.GetStringAsInt(Message, ref DataOffset, DataEnd);
            var query_text = BufferTokenizer.GetRemainingBytesAsString(Message, ref DataOffset, DataEnd);

            HandleQuery(query_flags, query_text);

            return true;
        }
    }
}

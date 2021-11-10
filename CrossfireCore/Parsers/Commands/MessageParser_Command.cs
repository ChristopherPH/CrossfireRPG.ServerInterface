using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParserBase
    {
        protected abstract void HandleCompletedCommand(UInt16 comc_packet, UInt32 comc_time);
        protected abstract void HandleQuery(int Flags, string QueryText);

        private void AddCommandParsers()
        {
            AddCommandHandler("comc", new ParseCommand(Parse_comc));
            AddCommandHandler("query", new ParseCommand(Parse_query));
        }

        private bool Parse_comc(byte[] packet, ref int offset, int end)
        {
            var comc_packet = BufferTokenizer.GetUInt16(packet, ref offset);
            var comc_time = BufferTokenizer.GetUInt32(packet, ref offset);
            //TODO: convert completed command time as it isn't quite correct

            HandleCompletedCommand(comc_packet, comc_time);

            return true;
        }

        private bool Parse_query(byte[] packet, ref int offset, int end)
        {
            var query_flags = BufferTokenizer.GetStringAsInt(packet, ref offset, end);
            var query_text = BufferTokenizer.GetRemainingBytesAsString(packet, ref offset, end);

            HandleQuery(query_flags, query_text);

            return true;
        }
    }
}

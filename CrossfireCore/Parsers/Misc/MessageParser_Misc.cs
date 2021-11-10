using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParserBase
    {
        protected abstract void HandlePickup(UInt32 PickupFlags);
        protected abstract void HandleReplyInfo(string request, byte[] reply);
        protected abstract void HandleTick(UInt32 TickCount);

        private void AddMiscParsers()
        {
            AddCommandHandler("pickup", new CommandParserDefinition(Parse_pickup));
            AddCommandHandler("replyinfo", new CommandParserDefinition(Parse_replyinfo));
            AddCommandHandler("tick", new CommandParserDefinition(Parse_tick, Common.Logger.Levels.Debug));
        }

        private bool Parse_pickup(byte[] packet, ref int offset, int end)
        {
            var pickup_mask = BufferTokenizer.GetUInt32(packet, ref offset);

            HandlePickup(pickup_mask);

            return true;
        }

        private bool Parse_replyinfo(byte[] packet, ref int offset, int end)
        {
            var reply_info = BufferTokenizer.GetString(packet, ref offset, end, BufferTokenizer.SpaceNewlineSeperator);
            var reply_bytes = BufferTokenizer.GetRemainingBytes(packet, ref offset, end);
            HandleReplyInfo(reply_info, reply_bytes);

            //TODO: bring in replyinfo object into this message parser

            return true;
        }

        private bool Parse_tick(byte[] packet, ref int offset, int end)
        {
            var tick_count = BufferTokenizer.GetUInt32(packet, ref offset);
            HandleTick(tick_count);

            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParser
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

        private bool Parse_pickup(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var pickup_mask = BufferTokenizer.GetUInt32(Message, ref DataOffset);

            HandlePickup(pickup_mask);

            return true;
        }

        private bool Parse_replyinfo(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var reply_info = BufferTokenizer.GetString(Message, ref DataOffset, DataEnd, BufferTokenizer.SpaceNewlineSeperator);
            var reply_bytes = BufferTokenizer.GetRemainingBytes(Message, ref DataOffset, DataEnd);
            HandleReplyInfo(reply_info, reply_bytes);

            //TODO: bring in replyinfo object into this message parser

            return true;
        }

        private bool Parse_tick(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var tick_count = BufferTokenizer.GetUInt32(Message, ref DataOffset);
            HandleTick(tick_count);

            return true;
        }
    }
}

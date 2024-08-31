using System;

namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageParser
    {
        protected abstract void HandlePickup(UInt32 PickupFlags);
        protected abstract void HandleTick(UInt32 TickCount);

        private void AddMiscParsers()
        {
            AddCommandHandler("pickup", new CommandParserDefinition(Parse_pickup));
            AddCommandHandler("tick", new CommandParserDefinition(Parse_tick, Common.Logger.Levels.Debug));
        }

        private bool Parse_pickup(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var pickup_mask = BufferTokenizer.GetUInt32(Message, ref DataOffset);

            HandlePickup(pickup_mask);

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

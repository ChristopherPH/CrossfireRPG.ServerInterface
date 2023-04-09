using System;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageBuilder
    {
        public bool SendInscribe(UInt32 SpellTag, UInt32 ScrollTag)
        {
            using (var ba = new BufferAssembler("inscribe"))
            {
                ba.AddByte(0);  //inscribe version, only 0 available
                ba.AddUInt32(SpellTag);
                ba.AddUInt32(ScrollTag);

                return SendMessage(ba);
            }
        }
    }
}

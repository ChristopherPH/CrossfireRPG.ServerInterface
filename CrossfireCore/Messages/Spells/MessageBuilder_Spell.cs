using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageBuilder
    {
        public void SendInscribe(UInt32 SpellTag, UInt32 ScrollTag)
        {
            using (var ba = new BufferAssembler("inscribe"))
            {
                ba.AddByte(0);  //inscribe version, only 0 available
                ba.AddUInt32(SpellTag);
                ba.AddUInt32(ScrollTag);

                SendMessage(ba);
            }
        }
    }
}

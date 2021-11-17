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
        public void SendAskFace(Int32 tag)
        {
            using (var ba = new BufferAssembler("askface"))
            {
                ba.AddIntAsString(tag);

                SendMessage(ba);
            }
        }
    }
}

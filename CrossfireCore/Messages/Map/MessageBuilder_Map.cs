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
        public void SendAskSmooth(Int32 tag)
        {
            using (var ba = new BufferAssembler("asksmooth"))
            {
                ba.AddIntAsString(tag);

                SendMessage(ba);
            }
        }

        public void SendLookAt(int x, int y)
        {
            using (var ba = new BufferAssembler("lookat"))
            {
                ba.AddString("{0} {1}", x, y);

                SendMessage(ba);
            }
        }
    }
}

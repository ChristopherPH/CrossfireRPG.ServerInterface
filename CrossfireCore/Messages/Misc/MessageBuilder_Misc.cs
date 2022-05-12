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
        public bool SendHeartbeat()
        {
            using (var ba = new BufferAssembler("beat", false))
            {
                return SendMessage(ba);
            }
        }
    }
}

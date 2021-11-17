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
        public UInt16 SendNewCommand(string command, UInt32 repeat = 0)
        {
            using (var ba = new BufferAssembler("ncom")) //NewCommand
            {
                var tmpPacket = nComPacket;

                ba.AddUInt16(tmpPacket);
                ba.AddUInt32(repeat);
                ba.AddString(command);

                nComPacket++;
                if (nComPacket == 0)    //don't allow a commandNumber of 0
                    nComPacket = 1;

                if (SendMessage(ba) == false)
                    return 0;   //no command

                return tmpPacket;
            }
        }

        /// <summary>
        /// Reply to a query
        /// </summary>
        /// <param name="Reply"></param>
        public void SendReply(string Reply)
        {
            using (var ba = new BufferAssembler("reply"))
            {
                ba.AddString(Reply);

                SendMessage(ba);
            }
        }
    }
}

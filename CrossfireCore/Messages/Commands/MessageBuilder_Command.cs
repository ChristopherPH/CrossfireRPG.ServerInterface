using System;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageBuilder
    {
        private UInt16 nComPacket = 1;

        /// <summary>
        /// Sends a NewCommand (UserCommand) to the server
        /// </summary>
        /// <returns>Command ID, or 0 on failure</returns>
        public UInt16 SendNewCommand(string command, Int32 repeat = 0)
        {
            using (var ba = new BufferAssembler("ncom")) //NewCommand
            {
                var tmpPacket = nComPacket;

                ba.AddUInt16(tmpPacket);
                ba.AddInt32(repeat);
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
        public bool SendReply(string Reply)
        {
            using (var ba = new BufferAssembler("reply"))
            {
                ba.AddString(Reply);

                return SendMessage(ba);
            }
        }
    }
}

using System;

namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageBuilder
    {
        public bool SendProtocolAskFace(Int32 tag)
        {
            using (var ba = new BufferAssembler("askface"))
            {
                ba.AddIntAsString(tag);

                return SendProtocolMessage(ba);
            }
        }
    }
}

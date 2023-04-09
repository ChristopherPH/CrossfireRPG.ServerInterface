using System;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageBuilder
    {
        public bool SendAskSmooth(Int32 tag)
        {
            using (var ba = new BufferAssembler("asksmooth"))
            {
                ba.AddIntAsString(tag);

                return SendMessage(ba);
            }
        }

        public bool SendLookAt(int x, int y)
        {
            using (var ba = new BufferAssembler("lookat"))
            {
                ba.AddString("{0} {1}", x, y);

                return SendMessage(ba);
            }
        }
    }
}

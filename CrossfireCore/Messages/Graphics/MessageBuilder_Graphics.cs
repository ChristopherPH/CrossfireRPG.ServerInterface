﻿using System;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageBuilder
    {
        public bool SendAskFace(Int32 tag)
        {
            using (var ba = new BufferAssembler("askface"))
            {
                ba.AddIntAsString(tag);

                return SendMessage(ba);
            }
        }
    }
}

/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System;

namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageBuilder
    {
        public bool SendProtocolAskFace(UInt32 tag)
        {
            /* askface only works with range 1-0xFFFF */
            if ((tag == 0) || (tag > 0xFFFF))
                return false;

            using (var ba = new BufferAssembler("askface"))
            {
                ba.AddIntAsString(tag);

                return SendProtocolMessage(ba);
            }
        }
    }
}

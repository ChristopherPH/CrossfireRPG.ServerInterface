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
        public bool SendProtocolInscribe(UInt32 SpellTag, UInt32 ScrollTag)
        {
            using (var ba = new BufferAssembler("inscribe"))
            {
                ba.AddByte(0);  //inscribe version, only 0 available
                ba.AddUInt32(SpellTag);
                ba.AddUInt32(ScrollTag);

                return SendProtocolMessage(ba);
            }
        }
    }
}

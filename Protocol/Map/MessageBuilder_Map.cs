﻿/*
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
        public bool SendProtocolAskSmooth(Int32 tag)
        {
            using (var ba = new BufferAssembler("asksmooth"))
            {
                ba.AddIntAsString(tag);

                return SendProtocolMessage(ba);
            }
        }

        /// <summary>
        /// Sends the lookat command to the server
        /// </summary>
        /// <param name="xoffset">Tile offset relative to player - player is at tile 0,0</param>
        /// <param name="yoffset">Tile Offset relative to player - player is at tile 0,0</param>
        /// <returns>true if the message was sent</returns>
        public bool SendProtocolLookAt(int xoffset, int yoffset)
        {
            using (var ba = new BufferAssembler("lookat"))
            {
                ba.AddString("{0} {1}", xoffset, yoffset);

                return SendProtocolMessage(ba);
            }
        }
    }
}

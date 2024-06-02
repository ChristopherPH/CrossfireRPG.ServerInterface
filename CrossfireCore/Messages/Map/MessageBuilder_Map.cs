﻿using System;

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

        /// <summary>
        /// Sends the lookat command to the server
        /// </summary>
        /// <param name="xoffset">Tile offset relative to player - player is at tile 0,0</param>
        /// <param name="yoffset">Tile Offset relative to player - player is at tile 0,0</param>
        /// <returns>true if the message was sent</returns>
        public bool SendLookAt(int xoffset, int yoffset)
        {
            using (var ba = new BufferAssembler("lookat"))
            {
                ba.AddString("{0} {1}", xoffset, yoffset);

                return SendMessage(ba);
            }
        }
    }
}

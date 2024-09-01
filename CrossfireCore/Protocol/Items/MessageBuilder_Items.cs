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
        /// <summary>
        /// Applies an object (Ready/Wield/Wear/Activate/Open/Close/Read/Eat/Use/...)
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>true if protocol message sent</returns>
        public bool SendProtocolApply(Int32 tag)
        {
            using (var ba = new BufferAssembler("apply"))
            {
                //Apply expects a signed Int32
                //Note that due to pseudo objects having the high bit of an 32bit int set,
                //on an x86 server a negative value must be sent for the server to be able
                //to parse said high bit.
                ba.AddIntAsString(tag);

                return SendProtocolMessage(ba);
            }
        }

        /// <summary>
        /// Examines an object
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>true if protocol message sent</returns>
        public bool SendProtocolExamine(Int32 tag)
        {
            using (var ba = new BufferAssembler("examine"))
            {
                //Examine expects a signed Int32
                //Note that due to pseudo objects having the high bit of an 32bit int set,
                //on an x86 server a negative value must be sent for the server to be able
                //to parse said high bit.
                ba.AddIntAsString(tag);

                return SendProtocolMessage(ba);
            }
        }

        /// <summary>
        /// Moves item(s)
        /// </summary>
        /// <param name="to">Destination item tag (container, player), 0=move to ground</param>
        /// <param name="tag">Object tag to move</param>
        /// <param name="nrof">Number of items to move, 0=all</param>
        /// <returns>true if protocol message sent</returns>
        public bool SendProtocolMove(Int32 to, Int32 tag, Int32 nrof = 0)
        {
            using (var ba = new BufferAssembler("move"))
            {
                ba.AddIntAsString(to);
                ba.AddSpace();
                ba.AddIntAsString(tag);
                ba.AddSpace();
                ba.AddIntAsString(nrof);

                return SendProtocolMessage(ba);
            }
        }

        /// <summary>
        /// Locks an item in the players inventory
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>true if protocol message sent</returns>
        public bool SendProtocolLock(UInt32 tag)
        {
            using (var ba = new BufferAssembler("lock"))
            {
                ba.AddByte((byte)1);
                ba.AddUInt32(tag);

                return SendProtocolMessage(ba);
            }
        }

        /// <summary>
        /// Unlocks an item in the players inventory
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>true if protocol message sent</returns>
        public bool SendProtocolUnlock(UInt32 tag)
        {
            using (var ba = new BufferAssembler("lock"))
            {
                ba.AddByte((byte)0);
                ba.AddUInt32(tag);

                return SendProtocolMessage(ba);
            }
        }

        /// <summary>
        /// Marks an item in the players inventory
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>true if protocol message sent</returns>
        public bool SendProtocolMark(UInt32 tag)
        {
            using (var ba = new BufferAssembler("mark"))
            {
                ba.AddUInt32(tag);

                return SendProtocolMessage(ba);
            }
        }
    }
}

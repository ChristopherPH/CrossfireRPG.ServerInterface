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
        private UInt16 nComPacket = 1;

        /// <summary>
        /// Sends a NewCommand (UserCommand) to the server
        /// </summary>
        /// <returns>Command ID, or 0 on failure</returns>
        public UInt16 SendProtocolNewCommand(string command, Int32 repeat = 0)
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

                if (SendProtocolMessage(ba) == false)
                    return 0;   //no command

                return tmpPacket;
            }
        }

        /// <summary>
        /// Reply to a query
        /// </summary>
        /// <param name="Reply"></param>
        public bool SendProtocolReply(string Reply)
        {
            using (var ba = new BufferAssembler("reply"))
            {
                ba.AddString(Reply);

                return SendProtocolMessage(ba);
            }
        }
    }
}

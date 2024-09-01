/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageBuilder
    {
        public bool SendProtocolRequestInfo(string Request)
        {
            using (var ba = new BufferAssembler("requestinfo"))
            {
                ba.AddString(Request);

                return SendProtocolMessage(ba);
            }
        }

        public bool SendProtocolRequestInfo(string Request, int level)
        {
            using (var ba = new BufferAssembler("requestinfo"))
            {
                ba.AddString(Request);
                ba.AddSpace();
                ba.AddIntAsString(level);

                return SendProtocolMessage(ba);
            }
        }
    }
}

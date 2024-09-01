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
        public bool SendProtocolVersion(int ClientToServer, int ServerToClient, string ClientName)
        {
            using (var ba = new BufferAssembler("version"))
            {
                ba.AddString("{0} {1} {2}", ClientToServer, ServerToClient, ClientName);

                return SendProtocolMessage(ba);
            }
        }

        public bool SendProtocolSetup(string SetupParameter, string SetupValue)
        {
            using (var ba = new BufferAssembler("setup"))
            {
                ba.AddString("{0} {1}", SetupParameter, SetupValue);

                return SendProtocolMessage(ba);
            }
        }

        public bool SendProtocolSetup(string SetupParameter, int SetupValue)
        {
            using (var ba = new BufferAssembler("setup"))
            {
                ba.AddString("{0} {1}", SetupParameter, SetupValue);

                return SendProtocolMessage(ba);
            }
        }

        public bool SendProtocolSetup(string SetupParameter, bool SetupValue)
        {
            using (var ba = new BufferAssembler("setup"))
            {
                ba.AddString("{0} {1}", SetupParameter, SetupValue ? 1 : 0);

                return SendProtocolMessage(ba);
            }
        }
    }
}

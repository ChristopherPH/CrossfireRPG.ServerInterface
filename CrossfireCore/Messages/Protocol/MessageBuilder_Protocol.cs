﻿using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageBuilder
    {
        public bool SendVersion(int ClientToServer, int ServerToClient, string ClientName)
        {
            using (var ba = new BufferAssembler("version"))
            {
                ba.AddString("{0} {1} {2}", ClientToServer, ServerToClient, ClientName);

                return SendMessage(ba);
            }
        }

        public bool SendSetup(string SetupParameter, string SetupValue)
        {
            using (var ba = new BufferAssembler("setup"))
            {
                ba.AddString("{0} {1}", SetupParameter, SetupValue);

                return SendMessage(ba);
            }
        }

        public bool SendSetup(string SetupParameter, int SetupValue)
        {
            using (var ba = new BufferAssembler("setup"))
            {
                ba.AddString("{0} {1}", SetupParameter, SetupValue);

                return SendMessage(ba);
            }
        }

        public bool SendSetup(string SetupParameter, bool SetupValue)
        {
            using (var ba = new BufferAssembler("setup"))
            {
                ba.AddString("{0} {1}", SetupParameter, SetupValue ? 1 : 0);

                return SendMessage(ba);
            }
        }
    }
}

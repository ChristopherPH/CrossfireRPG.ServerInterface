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
    public partial class MessageHandler
    {
        public event EventHandler<StartedCommandEventArgs> StartedCommand;
        public event EventHandler<CompletedCommandEventArgs> CompletedCommand;
        public event EventHandler<QueryEventArgs> Query;

        protected override void HandleStartedCommand(ushort startc_packet)
        {
            StartedCommand?.Invoke(this, new StartedCommandEventArgs()
            {
                Packet = startc_packet,
            });
        }

        protected override void HandleCompletedCommand(ushort comc_packet, uint comc_time)
        {
            CompletedCommand?.Invoke(this, new CompletedCommandEventArgs()
            {
                Packet = comc_packet,
                Time = comc_time,
            });
        }

        protected override void HandleQuery(int Flags, string QueryText)
        {
            Query?.Invoke(this, new QueryEventArgs()
            {
                Flags = Flags,
                QueryText = QueryText
            });
        }

        public class StartedCommandEventArgs : MessageHandlerEventArgs
        {
            public UInt16 Packet { get; set; }
        }

        public class CompletedCommandEventArgs : MessageHandlerEventArgs
        {
            public UInt16 Packet { get; set; }
            public UInt32 Time { get; set; }
        }

        public class QueryEventArgs : MessageHandlerEventArgs
        {
            public int Flags { get; set; }
            public string QueryText { get; set; }
        }
    }
}

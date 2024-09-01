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
        public event EventHandler<PickupEventArgs> Pickup;
        public event EventHandler<TickEventArgs> Tick;

        protected override void HandlePickup(UInt32 PickupFlags)
        {
            Pickup?.Invoke(this, new PickupEventArgs()
            {
                Flags = PickupFlags,
            });
        }

        protected override void HandleTick(UInt32 TickCount)
        {
            Tick?.Invoke(this, new TickEventArgs()
            {
                TickCount = TickCount,
            });
        }

        public class PickupEventArgs : MessageHandlerEventArgs
        {
            public UInt32 Flags { get; set; }
        }

        public class TickEventArgs : MessageHandlerEventArgs
        {
            public UInt32 TickCount { get; set; }
        }
    }
}

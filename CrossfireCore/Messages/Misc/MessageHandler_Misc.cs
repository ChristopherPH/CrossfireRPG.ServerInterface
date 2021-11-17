﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
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

        public class PickupEventArgs : SingleCommandEventArgs
        {
            public UInt32 Flags { get; set; }
        }

        public class TickEventArgs : SingleCommandEventArgs
        {
            public UInt32 TickCount { get; set; }
        }
    }
}

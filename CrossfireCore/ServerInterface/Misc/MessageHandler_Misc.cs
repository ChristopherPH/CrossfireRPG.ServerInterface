using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.Parser
{
    public partial class MessageHandler
    {
        public event EventHandler<PickupEventArgs> Pickup;
        public event EventHandler<ReplyInfoEventArgs> ReplyInfo;
        public event EventHandler<TickEventArgs> Tick;

        protected override void HandlePickup(UInt32 PickupFlags)
        {
            Pickup?.Invoke(this, new PickupEventArgs()
            {
                Flags = PickupFlags,
            });
        }

        protected override void HandleReplyInfo(string request, byte[] reply)
        {
            ReplyInfo?.Invoke(this, new ReplyInfoEventArgs()
            {
                Request = request,
                Reply = reply
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

        public class ReplyInfoEventArgs : SingleCommandEventArgs
        {
            public string Request { get; set; }
            public byte[] Reply { get; set; }
        }

        public class TickEventArgs : SingleCommandEventArgs
        {
            public UInt32 TickCount { get; set; }
        }
    }
}

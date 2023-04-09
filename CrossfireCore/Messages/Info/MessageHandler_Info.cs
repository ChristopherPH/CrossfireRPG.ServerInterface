using System;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageHandler
    {
        public event EventHandler<ReplyInfoEventArgs> ReplyInfo;

        protected override void HandleReplyInfo(string request, byte[] reply)
        {
            ReplyInfo?.Invoke(this, new ReplyInfoEventArgs()
            {
                Request = request,
                Reply = reply
            });
        }

        public class ReplyInfoEventArgs : SingleCommandEventArgs
        {
            public string Request { get; set; }
            public byte[] Reply { get; set; }
        }
    }
}

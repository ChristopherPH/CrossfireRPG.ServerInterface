using System;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageHandler
    {
        public event EventHandler<CompletedCommandEventArgs> CompletedCommand;
        public event EventHandler<QueryEventArgs> Query;

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

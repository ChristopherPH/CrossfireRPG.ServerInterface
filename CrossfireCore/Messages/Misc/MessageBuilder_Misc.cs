namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageBuilder
    {
        public bool SendProtocolHeartbeat()
        {
            using (var ba = new BufferAssembler("beat", false))
            {
                return SendProtocolMessage(ba);
            }
        }
    }
}

namespace CrossfireCore.ServerInterface
{
    public partial class MessageBuilder
    {
        public bool SendHeartbeat()
        {
            using (var ba = new BufferAssembler("beat", false))
            {
                return SendMessage(ba);
            }
        }
    }
}

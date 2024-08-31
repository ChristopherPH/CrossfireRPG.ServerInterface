namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageBuilder
    {
        public bool SendProtocolRequestInfo(string Request)
        {
            using (var ba = new BufferAssembler("requestinfo"))
            {
                ba.AddString(Request);

                return SendProtocolMessage(ba);
            }
        }

        public bool SendProtocolRequestInfo(string Request, int level)
        {
            using (var ba = new BufferAssembler("requestinfo"))
            {
                ba.AddString(Request);
                ba.AddSpace();
                ba.AddIntAsString(level);

                return SendProtocolMessage(ba);
            }
        }
    }
}

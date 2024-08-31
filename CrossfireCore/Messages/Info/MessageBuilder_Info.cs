namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageBuilder
    {
        public bool SendRequestInfo(string Request)
        {
            using (var ba = new BufferAssembler("requestinfo"))
            {
                ba.AddString(Request);

                return SendMessage(ba);
            }
        }

        public bool SendRequestInfo(string Request, int level)
        {
            using (var ba = new BufferAssembler("requestinfo"))
            {
                ba.AddString(Request);
                ba.AddSpace();
                ba.AddIntAsString(level);

                return SendMessage(ba);
            }
        }
    }
}

using System;

namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageHandler
    {
        public event EventHandler<AddKnowledgeEventArgs> AddKnowledge;

        protected override void HandleAddKnowledge(UInt32 ID, string Type, string Title, Int32 Face)
        {
            AddKnowledge?.Invoke(this, new AddKnowledgeEventArgs()
            {
                ID = ID,
                Type = Type,
                Title = Title,
                Face = Face
            });
        }

        public class AddKnowledgeEventArgs : BatchEventArgs
        {
            public UInt32 ID { get; set; }
            public string Type { get; set; }
            public string Title { get; set; }
            public Int32 Face { get; set; }
        }
    }
}

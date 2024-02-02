using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;

namespace CrossfireCore.Managers
{
    public class KnowledgeDataManager : DataListManager<UInt32, KnowledgeData, List<KnowledgeData>>
    {
        public KnowledgeDataManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Handler.AddKnowledge += Handler_AddKnowledge;
        }

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => true;
        public override DataModificationTypes SupportedModificationTypes =>
            base.SupportedModificationTypes | DataModificationTypes.Added;

        private void Handler_AddKnowledge(object sender, MessageHandler.AddKnowledgeEventArgs e)
        {
            AddDataObject(e.ID, new KnowledgeData()
            {
                KnowledgeID = e.ID,
                Type = e.Type,
                Title = e.Title,
                Face = e.Face,
            });
        }
    }

    public class KnowledgeData : DataObject
    {
        public UInt32 KnowledgeID { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public Int32 Face { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}

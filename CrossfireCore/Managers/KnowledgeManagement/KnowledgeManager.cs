using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;

namespace CrossfireCore.Managers.KnowledgeManagement
{
    public class KnowledgeManager : DataListManager<UInt32, Knowledge, List<Knowledge>>
    {
        public KnowledgeManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
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
            AddDataObject(e.ID, new Knowledge()
            {
                KnowledgeID = e.ID,
                Type = e.Type,
                Title = e.Title,
                Face = e.Face,
            });
        }
    }
}

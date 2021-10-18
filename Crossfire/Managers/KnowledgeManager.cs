using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.Managers
{
    public class KnowledgeManager : DataManager<Knowledge>
    {
        public KnowledgeManager(SocketConnection Connection, MessageBuilder Builder, MessageParser Parser)
            : base(Connection, Builder, Parser)
        {
            Parser.AddKnowledge += Parser_AddKnowledge;
        }

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => true;

        private void Parser_AddKnowledge(object sender, MessageParserBase.AddKnowledgeEventArgs e)
        {
            AddData(new Knowledge()
            {
                KnowledgeID = e.ID,
                Type = e.Type,
                Title = e.Title,
                Face = e.Face,
            });
        }
    }

    public class Knowledge
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

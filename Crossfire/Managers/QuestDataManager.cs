using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.Managers
{
    public class QuestDataManager : DataListManager<Quest>
    {
        public QuestDataManager(SocketConnection Connection, MessageBuilder Builder, MessageParser Parser)
            : base(Connection, Builder, Parser)
        {
            Parser.AddQuest += Parser_AddQuest;
            Parser.UpdateQuest += Parser_UpdateQuest;
        }

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => true;
        public override ModificationTypes SupportedModificationTypes =>
            base.SupportedModificationTypes | ModificationTypes.Added | ModificationTypes.Updated;

        private void Parser_AddQuest(object sender, MessageParser.AddQuestEventArgs e)
        {
            AddData(new Quest()
            {
                QuestID = e.Code,
                ParentID = e.Parent,
                Title = e.Title,
                Face = e.Face,
                Replay = e.Replay,
                End = e.End,
                Step = e.Step,
            });
        }

        private void Parser_UpdateQuest(object sender, MessageParser.UpdateQuestEventArgs e)
        {
            UpdateData(x => x.QuestID == e.Code, (data) =>
            {
                data.Step = e.Step;
                data.End = e.End;
                return new string[] { nameof(Quest.Step), nameof(Quest.End) };
            });
        }
    }

    public class Quest
    {
        public UInt32 QuestID { get; set; }
        public UInt32 ParentID { get; set; }
        public string Title { get; set; }
        public Int32 Face { get; set; }
        public byte Replay { get; set; }
        public byte End { get; set; }
        public string Step { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}

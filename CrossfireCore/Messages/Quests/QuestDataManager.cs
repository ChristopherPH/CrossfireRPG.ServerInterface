using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;

namespace CrossfireCore.Managers
{
    public class QuestDataManager : DataListManager<UInt32, Quest, List<Quest>>
    {
        public QuestDataManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Handler.AddQuest += Handler_AddQuest;
            Handler.UpdateQuest += Handler_UpdateQuest;
            Handler.BeginQuests += Handler_BeginQuests;
            Handler.EndQuests += Handler_EndQuests;
        }

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => true;
        public override DataModificationTypes SupportedModificationTypes =>
            base.SupportedModificationTypes | DataModificationTypes.Added | DataModificationTypes.Updated |
            DataModificationTypes.MultiCommandStart | DataModificationTypes.MultiCommandEnd;

        private void Handler_AddQuest(object sender, MessageHandler.AddQuestEventArgs e)
        {
            AddDataObject(e.Code, new Quest()
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

        private void Handler_UpdateQuest(object sender, MessageHandler.UpdateQuestEventArgs e)
        {
            UpdateDataObject(e.Code, (data) =>
            {
                data.Step = e.Step;
                data.End = e.End;
            });
        }

        private void Handler_BeginQuests(object sender, EventArgs e)
        {
            StartMultiCommand();
        }

        private void Handler_EndQuests(object sender, EventArgs e)
        {
            EndMultiCommand();
        }
    }

    public class Quest : DataObject
    {
        public UInt32 QuestID
        {
            get => _QuestID;
            set => SetProperty(ref _QuestID, value);
        }
        UInt32 _QuestID;

        public UInt32 ParentID
        {
            get => _ParentID;
            set => SetProperty(ref _ParentID, value);
        }
        UInt32 _ParentID;

        public string Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }
        string _Title;

        public Int32 Face
        {
            get => _Face;
            set => SetProperty(ref _Face, value);
        }
        Int32 _Face;

        public byte Replay
        {
            get => _Replay;
            set => SetProperty(ref _Replay, value);
        }
        byte _Replay;

        public byte End
        {
            get => _End;
            set => SetProperty(ref _End, value);
        }
        byte _End;

        public string Step
        {
            get => _Step;
            set => SetProperty(ref _Step, value);
        }
        string _Step;

        public override string ToString()
        {
            return string.Format("{0}:{1}: {2}", QuestID, ParentID, Title);
        }
    }
}

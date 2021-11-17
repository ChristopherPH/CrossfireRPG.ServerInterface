using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.Parser
{
    public partial class MessageHandler
    {
        public event EventHandler<AddKnowledgeEventArgs> AddKnowledge;
        public event EventHandler<AddQuestEventArgs> AddQuest;
        public event EventHandler<UpdateQuestEventArgs> UpdateQuest;

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

        protected override void HandleAddQuest(UInt32 Code, string Title, int Face,
            byte Replay, uint Parent, byte End, string Step)
        {
            AddQuest?.Invoke(this, new AddQuestEventArgs()
            {
                Code = Code,
                Title = Title,
                Face = Face,
                Replay = Replay,
                Parent = Parent,
                End = End,
                Step = Step,
            });
        }

        protected override void HandleUpdateQuest(UInt32 Code, byte End, string Step)
        {
            UpdateQuest?.Invoke(this, new UpdateQuestEventArgs()
            {
                Code = Code,
                End = End,
                Step = Step,
            });
        }

        public class AddKnowledgeEventArgs : MultiCommandEventArgs
        {
            public UInt32 ID { get; set; }
            public string Type { get; set; }
            public string Title { get; set; }
            public Int32 Face { get; set; }
        }

        public class AddQuestEventArgs : MultiCommandEventArgs
        {
            public UInt32 Code { get; set; }
            public string Title { get; set; }
            public Int32 Face { get; set; }
            public byte Replay { get; set; }
            public UInt32 Parent { get; set; }
            public byte End { get; set; }
            public string Step { get; set; }
        }

        public class UpdateQuestEventArgs : SingleCommandEventArgs
        {
            public UInt32 Code { get; set; }
            public byte End { get; set; }
            public string Step { get; set; }
        }
    }
}

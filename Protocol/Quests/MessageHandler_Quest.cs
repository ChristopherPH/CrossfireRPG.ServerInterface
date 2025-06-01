/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System;

namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageHandler
    {
        public event EventHandler<AddQuestEventArgs> AddQuest;
        public event EventHandler<UpdateQuestEventArgs> UpdateQuest;
        public event EventHandler<EventArgs> BeginQuests;
        public event EventHandler<EventArgs> EndQuests;

        protected override void HandleAddQuest(UInt32 Code, string Title, UInt32 Face,
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

        protected override void HandleBeginQuests()
        {
            BeginQuests?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandleEndQuests()
        {
            EndQuests?.Invoke(this, EventArgs.Empty);
        }

        public class AddQuestEventArgs : BatchEventArgs
        {
            public UInt32 Code { get; set; }
            public string Title { get; set; }
            public UInt32 Face { get; set; }
            public byte Replay { get; set; }
            public UInt32 Parent { get; set; }
            public byte End { get; set; }
            public string Step { get; set; }
        }

        public class UpdateQuestEventArgs : MessageHandlerEventArgs
        {
            public UInt32 Code { get; set; }
            public byte End { get; set; }
            public string Step { get; set; }
        }
    }
}

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

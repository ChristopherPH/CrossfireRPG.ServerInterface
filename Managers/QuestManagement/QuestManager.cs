/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using CrossfireRPG.ServerInterface.Network;
using CrossfireRPG.ServerInterface.Protocol;
using System;
using System.Collections.Generic;

namespace CrossfireRPG.ServerInterface.Managers.QuestManagement
{
    public class QuestManager : DataListManager<UInt32, Quest, List<Quest>>
    {
        public QuestManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
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
}

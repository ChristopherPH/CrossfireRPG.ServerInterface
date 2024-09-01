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

namespace CrossfireRPG.ServerInterface.Managers.KnowledgeManagement
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

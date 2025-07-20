/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using CrossfireRPG.ServerInterface.Network;
using CrossfireRPG.ServerInterface.Protocol;
using System.Collections.Generic;

namespace CrossfireRPG.ServerInterface.Managers.SkillManagement
{
    public class SkillManager : DataListManager<int, Skill, List<Skill>>
    {
        public SkillManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Handler.Skills += Handler_Skills;
            Handler.BeginSkills += Handler_BeginSkills;
            Handler.EndSkills += Handler_EndSkills;
        }

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => true;
        public override DataModificationTypes SupportedModificationTypes => base.SupportedModificationTypes | 
            DataModificationTypes.Added | DataModificationTypes.Updated |
            DataModificationTypes.MultiCommandStart | DataModificationTypes.MultiCommandEnd;

        private void Handler_Skills(object sender, MessageHandler.SkillEventArgs e)
        {
            if (!ContainsKey(e.Skill))
            {
                AddDataObject(e.Skill, new Skill()
                {
                    SkillID = e.Skill,
                    Level = e.Level,
                    Value = e.Value,
                    OldLevel = e.Level,
                    OldValue = e.Value,
                });
            }
            else
            {
                UpdateDataObject(e.Skill, (skillObject) =>
                {
                    skillObject.OldLevel = skillObject.Level;
                    skillObject.OldValue = skillObject.Value;
                    skillObject.Level = e.Level;
                    skillObject.Value = e.Value;
                });
            }
        }

        private void Handler_BeginSkills(object sender, BeginBatchEventArgs e)
        {
            StartMultiCommand();
        }

        private void Handler_EndSkills(object sender, EndBatchEventArgs e)
        {
            EndMultiCommand();
        }
    }
}

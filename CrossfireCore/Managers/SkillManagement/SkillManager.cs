using CrossfireCore.ServerInterface;
using CrossfireRPG.ServerInterface.Network;
using System;
using System.Collections.Generic;

namespace CrossfireCore.Managers.SkillManagement
{
    public class SkillManager : DataListManager<int, Skill, List<Skill>>
    {
        public SkillManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Handler.Skills += Handler_Skills;
        }

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => true;
        public override DataModificationTypes SupportedModificationTypes => base.SupportedModificationTypes | 
            DataModificationTypes.Added | DataModificationTypes.Updated;

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
    }
}

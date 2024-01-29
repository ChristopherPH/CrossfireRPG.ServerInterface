using CrossfireCore.ServerInterface;
using System;

namespace CrossfireCore.Managers
{
    public class SkillDataManager : DataListManager<Skill>
    {
        public SkillDataManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
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
            if (!Contains(x => x.SkillID == e.Skill))
            {
                AddData(new Skill()
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
                UpdateData(x => x.SkillID == e.Skill, (data) =>
                {
                    string[] rc = null;

                    if ((data.Level != e.Level) && (data.Value != e.Value))
                    {
                        rc = new string[] { nameof(Skill.Level), nameof(Skill.Value) };
                    }
                    else if (data.Level != e.Level)
                    {
                        rc = new string[] { nameof(Skill.Level) };
                    }
                    else if (data.Value != e.Value)
                    {
                        rc = new string[] { nameof(Skill.Value) };
                    }

                    data.OldLevel = data.Level;
                    data.OldValue = data.Value;
                    data.Level = e.Level;
                    data.Value = e.Value;
                    return rc;
                });
            }
        }
    }

    public class Skill
    {
        public int SkillID { get; set; }
        public Byte Level { get; set; }
        public UInt64 Value { get; set; }
        public Byte OldLevel { get; set; }
        public UInt64 OldValue { get; set; }
    }
}

using CrossfireCore.Parser;
using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.Managers
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
        public override ModificationTypes SupportedModificationTypes => base.SupportedModificationTypes | 
            ModificationTypes.Added | ModificationTypes.Updated;

        private void Handler_Skills(object sender, MessageHandler.SkillEventArgs e)
        {
            if (!Contains(x => x.SkillID == e.Skill))
            {
                AddData(new Skill()
                {
                    SkillID = e.Skill,
                    Level = e.Level,
                    Value = e.Value,
                });
            }
            else
            {
                UpdateData(x => x.SkillID == e.Skill, (data) =>
                {
                    data.Level = e.Level;
                    data.Value = e.Value;
                    return new string[] { nameof(Skill.Level), nameof(Skill.Value) };
                });
            }
        }
    }

    public class Skill
    {
        public int SkillID { get; set; }
        public Byte Level { get; set; }
        public UInt64 Value { get; set; }
    }
}

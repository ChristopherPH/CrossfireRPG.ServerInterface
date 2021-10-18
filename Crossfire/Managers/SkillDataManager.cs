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
        public SkillDataManager(SocketConnection Connection, MessageBuilder Builder, MessageParser Parser)
            : base(Connection, Builder, Parser)
        {
            Parser.Skills += Parser_Skills;
        }

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => true;
        public override ModificationTypes SupportedModificationTypes => base.SupportedModificationTypes | 
            ModificationTypes.Added | ModificationTypes.Updated;

        private void Parser_Skills(object sender, MessageParserBase.SkillEventArgs e)
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

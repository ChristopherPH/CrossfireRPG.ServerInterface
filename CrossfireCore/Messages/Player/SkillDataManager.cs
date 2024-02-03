using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;

namespace CrossfireCore.Managers
{
    public class SkillDataManager : DataListManager<int, Skill, List<Skill>>
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

    public class Skill : DataObject
    {
        public int SkillID
        {
            get => _SkillID;
            set => SetProperty(ref _SkillID, value);
        }
        private int _SkillID;

        public Byte Level
        {
            get => _Level;
            set => SetProperty(ref _Level, value);
        }
        private Byte _Level;

        public UInt64 Value
        {
            get => _Value;
            set => SetProperty(ref _Value, value);
        }
        private UInt64 _Value;

        public Byte OldLevel
        {
            get => _OldLevel;
            set => SetProperty(ref _OldLevel, value);
        }
        private Byte _OldLevel;

        public UInt64 OldValue
        {
            get => _OldValue;
            set => SetProperty(ref _OldValue, value);
        }
        private UInt64 _OldValue;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.Parser
{
    public partial class MessageHandler
    {
        public event EventHandler<PlayerEventArgs> Player;
        public event EventHandler<SkillEventArgs> Skills;
        public event EventHandler<StatEventArgs> Stats;
        public event EventHandler<EventArgs> BeginStats;
        public event EventHandler<EventArgs> EndStats;

        protected override void HandlePlayer(uint tag, uint weight, uint face, string Name)
        {
            Player?.Invoke(this, new PlayerEventArgs()
            {
                tag = tag,
                weight = weight,
                face = face,
                PlayerName = Name
            });
        }

        protected override void HandleSkill(int Skill, byte Level, UInt64 Value)
        {
            Skills?.Invoke(this, new SkillEventArgs()
            {
                Skill = Skill,
                Level = Level,
                Value = Value,
            });
        }

        protected override void HandleStat(NewClient.CharacterStats Stat, long Value)
        {
            Stats?.Invoke(this, new StatEventArgs()
            {
                Stat = Stat,
                Value = Value.ToString(),
            });
        }

        protected override void HandleStat(NewClient.CharacterStats Stat, string Value)
        {
            Stats?.Invoke(this, new StatEventArgs()
            {
                Stat = Stat,
                Value = Value,
            });
        }

        protected override void HandleStat(NewClient.CharacterStats Stat, float Value)
        {
            Stats?.Invoke(this, new StatEventArgs()
            {
                Stat = Stat,
                Value = Value.ToString(),
            });
        }

        protected override void HandleBeginStats()
        {
            BeginStats?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandleEndStats()
        {
            EndStats?.Invoke(this, EventArgs.Empty);
        }

        public class PlayerEventArgs : SingleCommandEventArgs
        {
            public UInt32 tag { get; set; }
            public UInt32 weight { get; set; }
            public UInt32 face { get; set; }
            public string PlayerName { get; set; }
        }

        public class SkillEventArgs : MultiCommandEventArgs
        {
            public int Skill { get; set; }
            public Byte Level { get; set; }
            public UInt64 Value { get; set; }
        }

        public class StatEventArgs : MultiCommandEventArgs
        {
            public NewClient.CharacterStats Stat { get; set; }
            public string Value { get; set; }
        }
    }
}

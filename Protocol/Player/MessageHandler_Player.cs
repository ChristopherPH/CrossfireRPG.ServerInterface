/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using CrossfireRPG.ServerInterface.Definitions;
using System;
using System.Collections.Generic;

namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageHandler
    {
        public event EventHandler<PlayerEventArgs> Player;
        public event EventHandler<SkillEventArgs> Skills;
        public event EventHandler<StatEventArgs> Stats;
        public event EventHandler<BeginBatchEventArgs> BeginSkills;
        public event EventHandler<EndBatchEventArgs> EndSkills;
        public event EventHandler<BeginBatchEventArgs> BeginStats;
        public event EventHandler<EndBatchEventArgs> EndStats;

        private Queue<SkillEventArgs> _SkillsBatchEvents = new Queue<SkillEventArgs>();
        private Queue<StatEventArgs> _StatsBatchEvents = new Queue<StatEventArgs>();

        protected override void HandlePlayer(uint tag, float weight, uint face, string Name)
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
            _SkillsBatchEvents.Enqueue(new SkillEventArgs()
            {
                Skill = Skill,
                Level = Level,
                Value = Value,
            });
        }

        protected override void HandleStatInt16(NewClient.CharacterStats Stat, Int16 Value)
        {
            _StatsBatchEvents.Enqueue(new StatEventArgs(Stat, Value));
        }

        protected override void HandleStatUInt16(NewClient.CharacterStats Stat, UInt16 Value)
        {
            _StatsBatchEvents.Enqueue(new StatEventArgs(Stat, Value));
        }

        protected override void HandleStatUInt32(NewClient.CharacterStats Stat, UInt32 Value)
        {
            _StatsBatchEvents.Enqueue(new StatEventArgs(Stat, Value));
        }

        protected override void HandleStatUInt64(NewClient.CharacterStats Stat, UInt64 Value)
        {
            _StatsBatchEvents.Enqueue(new StatEventArgs(Stat, Value));
        }

        protected override void HandleStatString(NewClient.CharacterStats Stat, string Value)
        {
            _StatsBatchEvents.Enqueue(new StatEventArgs(Stat, Value));
        }

        protected override void HandleStatFloat(NewClient.CharacterStats Stat, float Value)
        {
            _StatsBatchEvents.Enqueue(new StatEventArgs(Stat, Value));
        }

        protected override void HandleBeginStats()
        {
            _SkillsBatchEvents.Clear();
            _StatsBatchEvents.Clear();
        }

        protected override void HandleEndStats()
        {
            //Skills
            var BatchNumber = 1;
            var BatchCount = _SkillsBatchEvents.Count;

            BeginSkills?.Invoke(this, new BeginBatchEventArgs() { BatchCount = BatchCount });

            while (_SkillsBatchEvents.Count > 0)
            {
                var args = _SkillsBatchEvents.Dequeue();
                args.BatchNumber = BatchNumber++;
                args.BatchCount = BatchCount;
                Skills?.Invoke(this, args);
            }

            _SkillsBatchEvents.Clear();

            EndSkills?.Invoke(this, new EndBatchEventArgs() { BatchCount = BatchNumber });

            //Stats
            BatchNumber = 1;
            BatchCount = _StatsBatchEvents.Count;

            BeginStats?.Invoke(this, new BeginBatchEventArgs() { BatchCount = BatchCount });

            while (_StatsBatchEvents.Count > 0)
            {
                var args = _StatsBatchEvents.Dequeue();
                args.BatchNumber = BatchNumber++;
                args.BatchCount = BatchCount;
                Stats?.Invoke(this, args);
            }

            _StatsBatchEvents.Clear();

            EndStats?.Invoke(this, new EndBatchEventArgs() { BatchCount = BatchNumber });
        }

        public class PlayerEventArgs : MessageHandlerEventArgs
        {
            public UInt32 tag { get; set; }
            public float weight { get; set; }
            public UInt32 face { get; set; }
            public string PlayerName { get; set; }
        }

        public class SkillEventArgs : BatchEventArgs
        {
            public int Skill { get; set; }
            public Byte Level { get; set; }
            public UInt64 Value { get; set; }
        }

        public class StatEventArgs : BatchEventArgs
        {
            public StatEventArgs(NewClient.CharacterStats Stat, sbyte Value)
            {
                this.Stat = Stat;
                this.DataType = StatDataTypes.Int8;
                this.ValueInt8 = Value;
            }

            public StatEventArgs(NewClient.CharacterStats Stat, Int16 Value)
            {
                this.Stat = Stat;
                this.DataType = StatDataTypes.Int16;
                this.ValueInt16 = Value;
            }

            public StatEventArgs(NewClient.CharacterStats Stat, Int32 Value)
            {
                this.Stat = Stat;
                this.DataType = StatDataTypes.Int32;
                this.ValueInt32 = Value;
            }

            public StatEventArgs(NewClient.CharacterStats Stat, Int64 Value)
            {
                this.Stat = Stat;
                this.DataType = StatDataTypes.Int64;
                this.ValueInt64 = Value;
            }

            public StatEventArgs(NewClient.CharacterStats Stat, byte Value)
            {
                this.Stat = Stat;
                this.DataType = StatDataTypes.UInt8;
                this.ValueUInt8 = Value;
            }

            public StatEventArgs(NewClient.CharacterStats Stat, UInt16 Value)
            {
                this.Stat = Stat;
                this.DataType = StatDataTypes.UInt16;
                this.ValueUInt16 = Value;
            }

            public StatEventArgs(NewClient.CharacterStats Stat, UInt32 Value)
            {
                this.Stat = Stat;
                this.DataType = StatDataTypes.UInt32;
                this.ValueUInt32 = Value;
            }

            public StatEventArgs(NewClient.CharacterStats Stat, UInt64 Value)
            {
                this.Stat = Stat;
                this.DataType = StatDataTypes.UInt64;
                this.ValueUInt64 = Value;
            }

            public StatEventArgs(NewClient.CharacterStats Stat, float Value)
            {
                this.Stat = Stat;
                this.DataType = StatDataTypes.Float;
                this.ValueFloat = Value;
            }

            public StatEventArgs(NewClient.CharacterStats Stat, double Value)
            {
                this.Stat = Stat;
                this.DataType = StatDataTypes.Double;
                this.ValueDouble = Value;
            }

            public StatEventArgs(NewClient.CharacterStats Stat, string Value)
            {
                this.Stat = Stat;
                this.DataType = StatDataTypes.String;
                this.ValueString = Value;
            }

            public enum StatDataTypes
            {
                Int8,
                Int16,
                Int32,
                Int64,
                UInt8,
                UInt16,
                UInt32,
                UInt64,
                Float,
                Double,
                String,
            }

            public StatDataTypes DataType { get; }
            public NewClient.CharacterStats Stat { get; }
            public sbyte ValueInt8 { get; }
            public Int16 ValueInt16 { get; }
            public Int32 ValueInt32{ get; }
            public Int64 ValueInt64 { get; }
            public byte ValueUInt8 { get; }
            public UInt16 ValueUInt16 { get; }
            public UInt32 ValueUInt32 { get; }
            public UInt64 ValueUInt64 { get; }
            public float ValueFloat { get; }
            public double ValueDouble { get; }
            public string ValueString { get; }

            public string ValueAsString
            {
                get
                {
                    switch (DataType)
                    {
                        case StatDataTypes.Int8:
                            return this.ValueInt8.ToString();

                        case StatDataTypes.Int16:
                            return this.ValueInt16.ToString();

                        case StatDataTypes.Int32:
                            return this.ValueInt32.ToString();

                        case StatDataTypes.Int64:
                            return this.ValueInt64.ToString();

                        case StatDataTypes.UInt8:
                            return this.ValueUInt8.ToString();

                        case StatDataTypes.UInt16:
                            return this.ValueUInt16.ToString();

                        case StatDataTypes.UInt32:
                            return this.ValueUInt32.ToString();

                        case StatDataTypes.UInt64:
                            return this.ValueUInt64.ToString();

                        case StatDataTypes.Float:
                            return this.ValueFloat.ToString();

                        case StatDataTypes.Double:
                            return this.ValueDouble.ToString();

                        case StatDataTypes.String:
                            return this.ValueString.ToString();

                        default:
                            return string.Empty;
                    }
                }
            }
        }
    }
}

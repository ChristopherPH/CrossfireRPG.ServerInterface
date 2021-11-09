using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParserBase
    {
        protected abstract void HandlePlayer(UInt32 tag, UInt32 weight, UInt32 face, string Name);
        protected abstract void HandleSkill(int Skill, byte Level, UInt64 Value);
        protected abstract void HandleStat(NewClient.CharacterStats Stat, Int64 Value);
        protected abstract void HandleStat(NewClient.CharacterStats Stat, string Value);
        protected abstract void HandleStat(NewClient.CharacterStats Stat, float Value);

        private void AddPlayerParsers()
        {
            AddCommandHandler("player", new ParseCommand(Parse_player));
            AddCommandHandler("stats", new ParseCommand(Parse_stats));
        }

        private bool Parse_player(byte[] packet, ref int offset)
        {
            var player_tag = BufferTokenizer.GetUInt32(packet, ref offset);
            var player_weight = BufferTokenizer.GetUInt32(packet, ref offset);
            var player_face = BufferTokenizer.GetUInt32(packet, ref offset);
            var player_name_len = BufferTokenizer.GetByte(packet, ref offset);
            var player_name = BufferTokenizer.GetBytesAsString(packet, ref offset, player_name_len);

            HandlePlayer(player_tag, player_weight, player_face, player_name);

            return true;
        }

        private bool Parse_stats(byte[] packet, ref int offset)
        {
            while (offset < packet.Length)
            {
                var stat_number = BufferTokenizer.GetByte(packet, ref offset);

                switch ((NewClient.CharacterStats)stat_number)
                {
                    case NewClient.CharacterStats.Range:
                    case NewClient.CharacterStats.Title:
                    case NewClient.CharacterStats.GodName:
                        var stat_len = BufferTokenizer.GetByte(packet, ref offset);
                        var stat_text = BufferTokenizer.GetBytesAsString(packet, ref offset, stat_len);
                        HandleStat((NewClient.CharacterStats)stat_number, stat_text);
                        break;

                    case NewClient.CharacterStats.Speed:
                    case NewClient.CharacterStats.WeapSp:
                    case NewClient.CharacterStats.Overload:
                        var stat_32f = BufferTokenizer.GetUInt32(packet, ref offset);

                        HandleStat((NewClient.CharacterStats)stat_number, stat_32f / FLOAT_MULTF);
                        break;

                    case NewClient.CharacterStats.WeightLim: //technically a float, stat_32 / 1000
                        var stat_32 = BufferTokenizer.GetUInt32(packet, ref offset);
                        HandleStat((NewClient.CharacterStats)stat_number, stat_32);
                        break;

                    case NewClient.CharacterStats.Exp64:
                        var stat_64 = BufferTokenizer.GetUInt64(packet, ref offset);
                        HandleStat((NewClient.CharacterStats)stat_number, stat_64);
                        break;

                    case NewClient.CharacterStats.Hp:
                        var stat_16 = BufferTokenizer.GetUInt16(packet, ref offset);
                        HandleStat((NewClient.CharacterStats)stat_number, stat_16);
                        break;

                    case NewClient.CharacterStats.SpellAttune:
                    case NewClient.CharacterStats.SpellRepel:
                    case NewClient.CharacterStats.SpellDeny:
                    case NewClient.CharacterStats.CharacterFlags:
                        var stat_sp32 = BufferTokenizer.GetUInt32(packet, ref offset);
                        HandleStat((NewClient.CharacterStats)stat_number, stat_sp32);
                        break;

                    default:
                        if ((stat_number >= NewClient.CharacterStats_SkillInfo) &&
                            (stat_number < NewClient.CharacterStats_SkillInfo + NewClient.CharacterStats_NumSkills))
                        {
                            var skill_level = BufferTokenizer.GetByte(packet, ref offset);
                            var skill_value = BufferTokenizer.GetUInt64(packet, ref offset);
                            HandleSkill(stat_number, skill_level, skill_value);
                        }
                        else
                        {
                            var stat_value = BufferTokenizer.GetInt16(packet, ref offset);
                            HandleStat((NewClient.CharacterStats)stat_number, stat_value);
                        }
                        break;
                }
            }

            return true;
        }
    }
}

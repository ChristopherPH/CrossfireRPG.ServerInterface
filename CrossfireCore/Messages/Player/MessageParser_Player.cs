using System;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParser
    {
        protected abstract void HandlePlayer(UInt32 tag, float weight, UInt32 face, string Name);
        protected abstract void HandleSkill(int Skill, byte Level, UInt64 Value);
        protected abstract void HandleStatInt16(NewClient.CharacterStats Stat, Int16 Value);
        protected abstract void HandleStatUInt16(NewClient.CharacterStats Stat, UInt16 Value);
        protected abstract void HandleStatUInt32(NewClient.CharacterStats Stat, UInt32 Value);
        protected abstract void HandleStatUInt64(NewClient.CharacterStats Stat, UInt64 Value);
        protected abstract void HandleStatString(NewClient.CharacterStats Stat, string Value);
        protected abstract void HandleStatFloat(NewClient.CharacterStats Stat, float Value);
        protected abstract void HandleBeginStats();
        protected abstract void HandleEndStats();

        private void AddPlayerParsers()
        {
            AddCommandHandler("player", new CommandParserDefinition(Parse_player));
            AddCommandHandler("stats", new CommandParserDefinition(Parse_stats));
        }

        private bool Parse_player(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var player_tag = BufferTokenizer.GetUInt32(Message, ref DataOffset);
            var player_weight = BufferTokenizer.GetUInt32(Message, ref DataOffset) / FLOAT_MULTF; //UInt32 -> Float
            var player_face = BufferTokenizer.GetUInt32(Message, ref DataOffset);
            var player_name_len = BufferTokenizer.GetByte(Message, ref DataOffset);

            var player_name = string.Empty;
            if (player_name_len != 0)
                player_name = BufferTokenizer.GetBytesAsString(Message, ref DataOffset, player_name_len);

            HandlePlayer(player_tag, player_weight, player_face, player_name);

            return true;
        }

        private bool Parse_stats(byte[] Message, ref int DataOffset, int DataEnd)
        {
            HandleBeginStats();

            while (DataOffset < DataEnd)
            {
                var stat_number = BufferTokenizer.GetByte(Message, ref DataOffset);

                //Check if stat is actually a skill
                if ((stat_number >= NewClient.CharacterStats_SkillInfo) &&
                    (stat_number < NewClient.CharacterStats_SkillInfo + NewClient.CharacterStats_NumSkills))
                {
                    var skill_level = BufferTokenizer.GetByte(Message, ref DataOffset);
                    var skill_value = BufferTokenizer.GetUInt64(Message, ref DataOffset);

                    HandleSkill(stat_number, skill_level, skill_value);
                    continue;
                }

                //Get Stats
                switch ((NewClient.CharacterStats)stat_number)
                {
                    //Strings
                    case NewClient.CharacterStats.Range:
                    case NewClient.CharacterStats.Title:
                    case NewClient.CharacterStats.GodName:
                        var stat_len = BufferTokenizer.GetByte(Message, ref DataOffset);
                        var stat_text = BufferTokenizer.GetBytesAsString(Message, ref DataOffset, stat_len);
                        HandleStatString((NewClient.CharacterStats)stat_number, stat_text);
                        break;

                    //UInt16
                    case NewClient.CharacterStats.Hp:
                        var stat_16 = BufferTokenizer.GetUInt16(Message, ref DataOffset);
                        HandleStatUInt16((NewClient.CharacterStats)stat_number, stat_16);
                        break;

                    //UInt32
                    case NewClient.CharacterStats.SpellAttune:
                    case NewClient.CharacterStats.SpellRepel:
                    case NewClient.CharacterStats.SpellDeny:
                    case NewClient.CharacterStats.CharacterFlags:
                        var stat_sp32 = BufferTokenizer.GetUInt32(Message, ref DataOffset);
                        HandleStatUInt32((NewClient.CharacterStats)stat_number, stat_sp32);
                        break;

                    //UInt64
                    case NewClient.CharacterStats.Exp64:
                        var stat_64 = BufferTokenizer.GetUInt64(Message, ref DataOffset);
                        HandleStatUInt64((NewClient.CharacterStats)stat_number, stat_64);
                        break;

                    //Float
                    case NewClient.CharacterStats.Speed:
                    case NewClient.CharacterStats.WeapSp:
                    case NewClient.CharacterStats.Overload:
                    case NewClient.CharacterStats.WeightLim:
                        var stat_32f = BufferTokenizer.GetUInt32(Message, ref DataOffset);
                        HandleStatFloat((NewClient.CharacterStats)stat_number, stat_32f / FLOAT_MULTF);
                        break;

                    //Int16 (Default)
                    default:
                        var stat_value = BufferTokenizer.GetInt16(Message, ref DataOffset);
                        HandleStatInt16((NewClient.CharacterStats)stat_number, stat_value);
                        break;
                }
            }

            HandleEndStats();

            return true;
        }
    }
}

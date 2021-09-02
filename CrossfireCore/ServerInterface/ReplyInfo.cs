using CrossfireCore.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public class ReplyInfo
    {
        public ReplyInfo(MessageBuilder Builder, MessageParser Parser)
        {
            _Builder = Builder;
            _Parser = Parser;

            _Parser.ReplyInfo += _Parser_ReplyInfo;
        }

        private MessageBuilder _Builder;
        private MessageParser _Parser;

        //Server Info
        public string Motd { get; private set; }
        public string News { get; private set; }
        public string Rules { get; private set; }

        public Dictionary<int, (string Name, uint Face)> Skills { get; } = new Dictionary<int, (string Name, uint Face)>();
        public List<(uint Path, string Name)> SpellPaths { get; } = new List<(uint Path, string Name)>();
        public List<(string Type, string Name, Int32 Face, int Attempt)> Knowledges { get; } = new List<(string Type, string Name, Int32 Face, int Attempt)>();
        public UInt64[] ExperienceTable { get; private set; }

        //Helper functions
        public string GetSpellPath(uint Path)
        {
            string s = null;

            foreach (var i in SpellPaths)
            {
                if ((i.Path & Path) != 0)
                    s = s == null ? i.Name : s + ", " + i.Name;
            }

            return s ?? "";
        }

        public void RequestMOTD()
        {
            _Builder.SendRequestInfo("motd");
        }

        public void RequestNews()
        {
            _Builder.SendRequestInfo("news");
        }

        public void RequestRules()
        {
            _Builder.SendRequestInfo("rules");
        }

        public void RequestSkillInfo()
        {
            _Builder.SendRequestInfo("skill_info");
        }

        public void RequestExperienceTable()
        {
            _Builder.SendRequestInfo("exp_table");
        }

        public void RequestSpellPaths()
        {
            _Builder.SendRequestInfo("spell_paths");
        }

        public void RequestKnowledgeInfo()
        {
            _Builder.SendRequestInfo("knowledge_info");
        }

        public void RequestRaces()
        {
            _Builder.SendRequestInfo("race_list");
        }

        public void RequestClasses()
        {
            _Builder.SendRequestInfo("class_list");
        }

        private void _Parser_ReplyInfo(object sender, MessageParser.ReplyInfoEventArgs e)
        {
            int offset = 0;

            switch (e.Request)
            {
                case "motd":
                    this.Motd = BufferTokenizer.GetRemainingBytesAsString(e.Reply, ref offset);
                    break;

                case "news":
                    this.News = BufferTokenizer.GetRemainingBytesAsString(e.Reply, ref offset);
                    break;

                case "rules":
                    this.Rules = BufferTokenizer.GetRemainingBytesAsString(e.Reply, ref offset);
                    break;

                case "skill_info":
                    while (offset < e.Reply.Length)
                    {
                        var SkillSet = BufferTokenizer.GetString(e.Reply, ref offset, new byte[] { 0x0A }).Split(':');
                        var SkillID = int.Parse(SkillSet[0]);
                        switch (SkillSet.Length)
                        {
                            case 2: Skills[SkillID] = (SkillSet[1].ToTitleCase(), 0); break;
                            case 3: Skills[SkillID] = (SkillSet[1].ToTitleCase(), UInt32.Parse(SkillSet[2])); break;
                            default: throw new Exception();
                        }
                    }
                    break;

                case "exp_table":
                    var NumLevels = BufferTokenizer.GetUInt16(e.Reply, ref offset);
                    ExperienceTable = new UInt64[NumLevels];

                    for (int i = 1; i < NumLevels; i++)
                        ExperienceTable[i] = BufferTokenizer.GetUInt64(e.Reply, ref offset);
                    break;

                case "spell_paths":
                    while (offset < e.Reply.Length)
                    {
                        var SpellPathData = BufferTokenizer.GetString(e.Reply, ref offset, new byte[] { 0x0A }).Split(':');

                        var SpellPathID = uint.Parse(SpellPathData[0]);
                        var SpellPathName = SpellPathData[1].ToTitleCase();

                        SpellPaths.Add((SpellPathID, SpellPathName));
                    }
                    break;

                case "knowledge_info":
                    while (offset < e.Reply.Length)
                    {
                        var knowledge_info = BufferTokenizer.GetString(e.Reply, ref offset, new byte[] { 0x0A }).Split(':');

                        var knowledge_type = knowledge_info[0];
                        var knowledge_name = knowledge_info[1];
                        var knowledge_face = int.Parse(knowledge_info[2]);
                        var knowledge_can_attempt = int.Parse(knowledge_info[3]);

                        Knowledges.Add((knowledge_type, knowledge_name, knowledge_face, knowledge_can_attempt));
                    }
                    break;

                case "race_list":
                    var race_arches = BufferTokenizer.GetRemainingBytesAsString(e.Reply, ref offset).Split('|');
                    foreach (var race in race_arches)
                        if (!string.IsNullOrWhiteSpace(race))
                            _Builder.SendRequestInfo("race_info " + race);
                    break;

                case "race_info":
                    var race_arch = BufferTokenizer.GetString(e.Reply, ref offset, BufferTokenizer.NewlineSeperator);

                    while (offset < e.Reply.Length)
                    {
                        var race_info_key = BufferTokenizer.GetString(e.Reply, ref offset);

                        switch (race_info_key)
                        {
                            case "name":
                                var race_name_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                var race_name = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, race_name_len);
                                break;

                            case "msg":
                                var race_desc_len = BufferTokenizer.GetUInt16(e.Reply, ref offset);
                                var race_desc = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, race_desc_len);
                                break;

                            case "stats":
                                var race_stats_number = BufferTokenizer.GetByte(e.Reply, ref offset);
                                while (race_stats_number != 0)
                                {
                                    switch ((NewClient.CharacterStats)race_stats_number)
                                    {
                                        case NewClient.CharacterStats.Range:
                                        case NewClient.CharacterStats.Title:
                                            var stat_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                            var stat_text = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, stat_len);
                                            break;

                                        case NewClient.CharacterStats.Speed:
                                        case NewClient.CharacterStats.WeapSp:
                                            var stat_32f = BufferTokenizer.GetUInt32(e.Reply, ref offset); ;
                                            break;

                                        case NewClient.CharacterStats.WeightLim:
                                            var stat_32 = BufferTokenizer.GetUInt32(e.Reply, ref offset);
                                            break;

                                        case NewClient.CharacterStats.Exp64:
                                            var stat_64 = BufferTokenizer.GetUInt64(e.Reply, ref offset);
                                            break;

                                        case NewClient.CharacterStats.Hp:
                                            var stat_16 = BufferTokenizer.GetUInt16(e.Reply, ref offset);
                                            break;

                                        case NewClient.CharacterStats.SpellAttune:
                                        case NewClient.CharacterStats.SpellRepel:
                                        case NewClient.CharacterStats.SpellDeny:
                                            var stat_sp32 = BufferTokenizer.GetUInt32(e.Reply, ref offset);
                                            break;

                                        default:
                                            if ((race_stats_number >= NewClient.CharacterStats_SkillInfo) &&
                                                (race_stats_number < NewClient.CharacterStats_SkillInfo + NewClient.CharacterStats_NumSkills))
                                            {
                                                var skill_level = BufferTokenizer.GetByte(e.Reply, ref offset);
                                                var skill_value = BufferTokenizer.GetUInt64(e.Reply, ref offset);
                                            }
                                            else
                                            {
                                                var stat_value = BufferTokenizer.GetUInt16(e.Reply, ref offset);
                                            }
                                            break;
                                    }

                                    race_stats_number = BufferTokenizer.GetByte(e.Reply, ref offset);
                                }
                                break;

                            case "choice":
                                var race_choice_name_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                var race_choice_name = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, race_choice_name_len);
                                var race_choice_desc_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                var race_choice_desc = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, race_choice_desc_len);

                                var race_choice_arch_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                while (race_choice_arch_len != 0)
                                {
                                    var race_choice_arch = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, race_choice_arch_len);
                                    var race_choice_arch_desc_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                    var race_choice_arch_desc = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, race_choice_arch_desc_len);

                                    race_choice_arch_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                }
                                break;
                        }
                    }

                    break;

                case "class_list":
                    var class_arches = BufferTokenizer.GetRemainingBytesAsString(e.Reply, ref offset).Split('|');
                    foreach (var cls in class_arches)
                        if (!string.IsNullOrWhiteSpace(cls))
                            _Builder.SendRequestInfo("class_info " + cls);
                    break;

                case "class_info":
                    var class_arch = BufferTokenizer.GetString(e.Reply, ref offset, BufferTokenizer.NewlineSeperator);

                    while (offset < e.Reply.Length)
                    {
                        var class_info_key = BufferTokenizer.GetString(e.Reply, ref offset);

                        switch (class_info_key)
                        {
                            case "name":
                                var class_name_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                var class_name = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, class_name_len);
                                break;

                            case "msg":
                                var class_desc_len = BufferTokenizer.GetUInt16(e.Reply, ref offset);
                                var class_desc = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, class_desc_len);
                                break;

                            case "stats":
                                var class_stats_number = BufferTokenizer.GetByte(e.Reply, ref offset);
                                while (class_stats_number != 0)
                                {
                                    switch ((NewClient.CharacterStats)class_stats_number)
                                    {
                                        case NewClient.CharacterStats.Range:
                                        case NewClient.CharacterStats.Title:
                                            var stat_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                            var stat_text = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, stat_len);
                                            break;

                                        case NewClient.CharacterStats.Speed:
                                        case NewClient.CharacterStats.WeapSp:
                                            var stat_32f = BufferTokenizer.GetUInt32(e.Reply, ref offset); ;
                                            break;

                                        case NewClient.CharacterStats.WeightLim:
                                            var stat_32 = BufferTokenizer.GetUInt32(e.Reply, ref offset);
                                            break;

                                        case NewClient.CharacterStats.Exp64:
                                            var stat_64 = BufferTokenizer.GetUInt64(e.Reply, ref offset);
                                            break;

                                        case NewClient.CharacterStats.Hp:
                                            var stat_16 = BufferTokenizer.GetUInt16(e.Reply, ref offset);
                                            break;

                                        case NewClient.CharacterStats.SpellAttune:
                                        case NewClient.CharacterStats.SpellRepel:
                                        case NewClient.CharacterStats.SpellDeny:
                                            var stat_sp32 = BufferTokenizer.GetUInt32(e.Reply, ref offset);
                                            break;

                                        default:
                                            if ((class_stats_number >= NewClient.CharacterStats_SkillInfo) &&
                                                (class_stats_number < NewClient.CharacterStats_SkillInfo + NewClient.CharacterStats_NumSkills))
                                            {
                                                var skill_level = BufferTokenizer.GetByte(e.Reply, ref offset);
                                                var skill_value = BufferTokenizer.GetUInt64(e.Reply, ref offset);
                                            }
                                            else
                                            {
                                                var stat_value = BufferTokenizer.GetUInt16(e.Reply, ref offset);
                                            }
                                            break;
                                    }

                                    class_stats_number = BufferTokenizer.GetByte(e.Reply, ref offset);
                                }
                                break;

                            case "choice":
                                var class_choice_name_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                var class_choice_name = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, class_choice_name_len);
                                var class_choice_desc_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                var class_choice_desc = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, class_choice_desc_len);

                                var class_choice_arch_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                while (class_choice_arch_len != 0)
                                {
                                    var class_choice_arch = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, class_choice_arch_len);
                                    var class_choice_arch_desc_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                    var class_choice_arch_desc = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, class_choice_arch_desc_len);

                                    class_choice_arch_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                }
                                break;
                        }
                    }
                    break;

                default:
                    return;
            }

            if (offset < e.Reply.Length)
            {
                Logger.Log(Logger.Levels.Warn, "Excess Data for ReplyInfo {0}:\n{1}",
                    e.Request, HexDump.Utils.HexDump(e.Reply, offset));
            }
        }

    }
}

using Common;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public class ReplyInfo
    {
        static Logger _Logger = new Logger(nameof(ReplyInfo));

        public ReplyInfo(MessageBuilder Builder, MessageParser Parser)
        {
            _Builder = Builder;
            _Parser = Parser;

            _Parser.ReplyInfo += _Parser_ReplyInfo;
        }

        private MessageBuilder _Builder;
        private MessageParser _Parser;

        public event EventHandler<InfoAvailableEventArgs> InfoAvailable;

        //Server Info
        public string Motd { get; private set; }
        public string News { get; private set; }
        public string Rules { get; private set; }

        public Dictionary<int, Skill> Skills { get; } = new Dictionary<int, Skill>();
        public List<SpellPath> SpellPaths { get; } = new List<SpellPath>();
        public List<KnowledgeInfo> KnowledgeInfos { get; } = new List<KnowledgeInfo>();
        public UInt64[] ExperienceTable { get; private set; }

        public Dictionary<string, RaceClassInfo> Races { get; } = new Dictionary<string, RaceClassInfo>();
        public Dictionary<string, RaceClassInfo> Classes { get; } = new Dictionary<string, RaceClassInfo>();
        public List<StartingMap> StartingMaps { get; } = new List<StartingMap>();

        public NewCharInfo NewCharacterInfo { get; private set; } = new NewCharInfo();

        public class KnowledgeInfo
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public int Face { get; set; }
            public int CanAttempt { get; set; }
        }
        public class Skill
        {
            public string Name { get; set; }
            public uint Face { get; set; }
        }

        public class SpellPath
        {
            public uint Path { get; set; }
            public string Name { get; set; }
        }

        public class RaceClassChoice
        {
            public string ArchName { get; set; } = string.Empty;
            public string ArchDescription { get; set; } = string.Empty;
        }

        public class RaceClassStat
        {
            public NewClient.CharacterStats Stat { get; set; }
            public string StatValue { get; set; } = string.Empty;
        }

        public class RaceClassInfo
        {
            public string Arch { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string ChoiceName { get; set; } = string.Empty;
            public string ChoiceDescription { get; set; } = string.Empty;

            public List<RaceClassStat> Stats { get; set; } = new List<RaceClassStat>();
            public List<RaceClassChoice> Choices { get; set; } = new List<RaceClassChoice>();
        }

        public class StartingMap
        {
            public string Arch { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        public class NewCharInfo
        {
            public int Points { get; set; }
            public int StatMin { get; set; }
            public int StatMax { get; set; }
            public List<string> StatNames { get; set; } = new List<string>();
            public List<NewCharInfoValues> Values { get; } = new List<NewCharInfoValues>();
        }

        public class NewCharInfoValues
        {
            public enum NewCharInfoTypes
            {
                Required = 'R',
                Optional = 'O',
                Values = 'V',
                Information = 'I'
            }

            public NewCharInfoTypes InfoType { get; set; }
            public string Variable { get; set; }
            public string Values { get; set; }
        }

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

        //Supported Request functions
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

        public void RequestStartingMap()
        {
            _Builder.SendRequestInfo("startingmap");
        }

        public void RequestNewCharInfo()
        {
            _Builder.SendRequestInfo("newcharinfo");
        }

        public void ClearReplyInfo()
        {
            Motd = string.Empty;
            News = string.Empty;
            Rules = string.Empty;
            Skills.Clear();
            SpellPaths.Clear();
            KnowledgeInfos.Clear();
            ExperienceTable = null;
            Races.Clear();
            Classes.Clear();
            StartingMaps.Clear();
            NewCharacterInfo = new NewCharInfo();
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
                    Skills.Clear();
                    while (offset < e.Reply.Length)
                    {
                        var SkillSet = BufferTokenizer.GetString(e.Reply, ref offset, new byte[] { 0x0A }).Split(':');
                        var SkillID = int.Parse(SkillSet[0]);
                        switch (SkillSet.Length)
                        {
                            case 2: Skills[SkillID] = new Skill() { Name = SkillSet[1].ToTitleCase(), Face = 0 }; break;
                            case 3: Skills[SkillID] = new Skill() { Name = SkillSet[1].ToTitleCase(), Face = UInt32.Parse(SkillSet[2]) }; break;
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
                    SpellPaths.Clear();
                    while (offset < e.Reply.Length)
                    {
                        var SpellPathData = BufferTokenizer.GetString(e.Reply, ref offset, new byte[] { 0x0A }).Split(':');

                        var SpellPathID = uint.Parse(SpellPathData[0]);
                        var SpellPathName = SpellPathData[1].ToTitleCase();

                        SpellPaths.Add(new SpellPath() { Path = SpellPathID, Name = SpellPathName });
                    }
                    break;

                case "knowledge_info":
                    KnowledgeInfos.Clear();
                    while (offset < e.Reply.Length)
                    {
                        var knowledge_info = BufferTokenizer.GetString(e.Reply, ref offset, new byte[] { 0x0A }).Split(':');

                        var knowledge_type = knowledge_info[0];
                        var knowledge_name = knowledge_info[1];
                        var knowledge_face = int.Parse(knowledge_info[2]);
                        var knowledge_can_attempt = int.Parse(knowledge_info[3]);

                        KnowledgeInfos.Add(new KnowledgeInfo()
                        {
                            Type = knowledge_type,
                            Name = knowledge_name,
                            Face = knowledge_face,
                            CanAttempt = knowledge_can_attempt,
                        });
                    }
                    break;

                case "race_list":
                    this.Races.Clear();
                    var race_arches = BufferTokenizer.GetRemainingBytesAsString(e.Reply, ref offset).Split('|');
                    foreach (var race in race_arches)
                        if (!string.IsNullOrWhiteSpace(race))
                            _Builder.SendRequestInfo("race_info " + race);
                    break;

                case "race_info":
                    var race_arch = BufferTokenizer.GetString(e.Reply, ref offset, BufferTokenizer.NewlineSeperator);

                    this.Races[race_arch] = new RaceClassInfo()
                    {
                        Arch = race_arch
                    };

                    while (offset < e.Reply.Length)
                    {
                        var race_info_key = BufferTokenizer.GetString(e.Reply, ref offset);

                        switch (race_info_key)
                        {
                            case "name":
                                var race_name_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                this.Races[race_arch].Name = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, race_name_len);
                                break;

                            case "msg":
                                var race_desc_len = BufferTokenizer.GetUInt16(e.Reply, ref offset);
                                this.Races[race_arch].Description = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, race_desc_len);
                                break;

                            case "stats":
                                var race_stats_number = BufferTokenizer.GetByte(e.Reply, ref offset);
                                while (race_stats_number != 0)
                                {
                                    var race_stat_value = string.Empty;

                                    switch ((NewClient.CharacterStats)race_stats_number)
                                    {
                                        case NewClient.CharacterStats.Range:
                                        case NewClient.CharacterStats.Title:
                                            var stat_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                            race_stat_value = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, stat_len);
                                            break;

                                        case NewClient.CharacterStats.Speed:
                                        case NewClient.CharacterStats.WeapSp:
                                            race_stat_value = BufferTokenizer.GetUInt32(e.Reply, ref offset).ToString();
                                            break;

                                        case NewClient.CharacterStats.WeightLim:
                                            race_stat_value = BufferTokenizer.GetUInt32(e.Reply, ref offset).ToString();
                                            break;

                                        case NewClient.CharacterStats.Exp64:
                                            race_stat_value = BufferTokenizer.GetUInt64(e.Reply, ref offset).ToString();
                                            break;

                                        case NewClient.CharacterStats.Hp:
                                            race_stat_value = BufferTokenizer.GetUInt16(e.Reply, ref offset).ToString();
                                            break;

                                        case NewClient.CharacterStats.SpellAttune:
                                        case NewClient.CharacterStats.SpellRepel:
                                        case NewClient.CharacterStats.SpellDeny:
                                            race_stat_value = BufferTokenizer.GetUInt32(e.Reply, ref offset).ToString();
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
                                                race_stat_value = BufferTokenizer.GetInt16(e.Reply, ref offset).ToString();
                                            }
                                            break;
                                    }

                                    this.Races[race_arch].Stats.Add(new RaceClassStat()
                                    {
                                         Stat = (NewClient.CharacterStats)race_stats_number,
                                         StatValue = race_stat_value,
                                    });

                                    race_stats_number = BufferTokenizer.GetByte(e.Reply, ref offset);
                                }
                                break;

                            case "choice":
                                var race_choice_name_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                this.Races[race_arch].ChoiceName = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, race_choice_name_len);

                                var race_choice_desc_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                this.Races[race_arch].ChoiceDescription = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, race_choice_desc_len);

                                var race_choice_arch_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                while (race_choice_arch_len != 0)
                                {
                                    var choicePair = new RaceClassChoice();
                                    this.Races[race_arch].Choices.Add(choicePair);

                                    choicePair.ArchName = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, race_choice_arch_len);
                                    var race_choice_arch_desc_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                    choicePair.ArchDescription = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, race_choice_arch_desc_len);

                                    race_choice_arch_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                }
                                break;
                        }
                    }

                    break;

                case "class_list":
                    this.Classes.Clear();
                    var class_arches = BufferTokenizer.GetRemainingBytesAsString(e.Reply, ref offset).Split('|');
                    foreach (var cls in class_arches)
                        if (!string.IsNullOrWhiteSpace(cls))
                            _Builder.SendRequestInfo("class_info " + cls);
                    break;

                case "class_info":
                    var class_arch = BufferTokenizer.GetString(e.Reply, ref offset, BufferTokenizer.NewlineSeperator);

                    this.Classes[class_arch] = new RaceClassInfo()
                    {
                        Arch = class_arch
                    };

                    while (offset < e.Reply.Length)
                    {
                        var class_info_key = BufferTokenizer.GetString(e.Reply, ref offset);

                        switch (class_info_key)
                        {
                            case "name":
                                var class_name_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                this.Classes[class_arch].Name = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, class_name_len);
                                break;

                            case "msg":
                                var class_desc_len = BufferTokenizer.GetUInt16(e.Reply, ref offset);
                                this.Classes[class_arch].Description = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, class_desc_len);
                                break;

                            case "stats":
                                var class_stats_number = BufferTokenizer.GetByte(e.Reply, ref offset);
                                while (class_stats_number != 0)
                                {
                                    var class_stat_value = string.Empty;

                                    switch ((NewClient.CharacterStats)class_stats_number)
                                    {
                                        case NewClient.CharacterStats.Range:
                                        case NewClient.CharacterStats.Title:
                                            var stat_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                            class_stat_value = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, stat_len);
                                            break;

                                        case NewClient.CharacterStats.Speed:
                                        case NewClient.CharacterStats.WeapSp:
                                            class_stat_value = BufferTokenizer.GetUInt32(e.Reply, ref offset).ToString();
                                            break;

                                        case NewClient.CharacterStats.WeightLim:
                                            class_stat_value = BufferTokenizer.GetUInt32(e.Reply, ref offset).ToString();
                                            break;

                                        case NewClient.CharacterStats.Exp64:
                                            class_stat_value = BufferTokenizer.GetUInt64(e.Reply, ref offset).ToString();
                                            break;

                                        case NewClient.CharacterStats.Hp:
                                            class_stat_value = BufferTokenizer.GetUInt16(e.Reply, ref offset).ToString();
                                            break;

                                        case NewClient.CharacterStats.SpellAttune:
                                        case NewClient.CharacterStats.SpellRepel:
                                        case NewClient.CharacterStats.SpellDeny:
                                            class_stat_value = BufferTokenizer.GetUInt32(e.Reply, ref offset).ToString();
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
                                                class_stat_value = BufferTokenizer.GetInt16(e.Reply, ref offset).ToString();
                                            }
                                            break;
                                    }

                                    this.Classes[class_arch].Stats.Add(new RaceClassStat()
                                    {
                                        Stat = (NewClient.CharacterStats)class_stats_number,
                                        StatValue = class_stat_value,
                                    });

                                    class_stats_number = BufferTokenizer.GetByte(e.Reply, ref offset);
                                }
                                break;

                            case "choice":
                                var class_choice_name_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                this.Classes[class_arch].ChoiceName = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, class_choice_name_len);

                                var class_choice_desc_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                this.Classes[class_arch].ChoiceDescription = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, class_choice_desc_len);

                                var class_choice_arch_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                while (class_choice_arch_len != 0)
                                {
                                    var choicePair = new RaceClassChoice();
                                    this.Classes[class_arch].Choices.Add(choicePair);

                                    choicePair.ArchName = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, class_choice_arch_len);
                                    var class_choice_arch_desc_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                    choicePair.ArchDescription = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, class_choice_arch_desc_len);

                                    class_choice_arch_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                                }
                                break;
                        }
                    }
                    break;

                case "startingmap":
                    StartingMaps.Clear();
                    StartingMap startingMap = null;

                    while (offset < e.Reply.Length)
                    {
                        var starting_map_type = BufferTokenizer.GetByte(e.Reply, ref offset);
                        var starting_map_data_len = BufferTokenizer.GetInt16(e.Reply, ref offset);
                        var starting_map_data = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, starting_map_data_len);

                        switch (starting_map_type)
                        {
                            case 1: //INFO_MAP_ARCH_NAME
                                if (startingMap != null)
                                    StartingMaps.Add(startingMap);

                                startingMap = new StartingMap()
                                {
                                    Arch = starting_map_data
                                };
                                break;

                            case 2: //INFO_MAP_NAME
                                startingMap.Name = starting_map_data;
                                break;

                            case 3: //INFO_MAP_DESCRIPTION
                                startingMap.Description = starting_map_data;
                                break;
                        }
                    }

                    if (startingMap != null)
                        StartingMaps.Add(startingMap);
                    break;

                case "newcharinfo":
                    this.NewCharacterInfo = new NewCharInfo();

                    while (offset < e.Reply.Length)
                    {
                        var newcharinfo_len = BufferTokenizer.GetByte(e.Reply, ref offset);
                        if (newcharinfo_len == 0)
                            continue;

                        //get the string without the null terminator
                        var newcharinfo_line = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, newcharinfo_len - 1);
                        BufferTokenizer.GetByte(e.Reply, ref offset); //skip null terminator

                        var newcharinfo_type = newcharinfo_line[0];
                        var newcharinfo_variable = newcharinfo_line.Substring(2, newcharinfo_line.IndexOf(' ', 2) - 2);
                        var newcharinfo_value = newcharinfo_line.Substring(newcharinfo_variable.Length + 3);

                        var newcharinfovalues = new NewCharInfoValues()
                        {
                            InfoType = (NewCharInfoValues.NewCharInfoTypes)newcharinfo_type,
                            Variable = newcharinfo_variable,
                            Values = newcharinfo_value
                        };

                        NewCharacterInfo.Values.Add(newcharinfovalues);

                        switch (newcharinfo_variable)
                        {
                            case "points":
                                this.NewCharacterInfo.Points = int.Parse(newcharinfo_value);
                                break;

                            case "statrange":
                                var newcharinfo_range = newcharinfo_value.Split(' ');
                                this.NewCharacterInfo.StatMin = int.Parse(newcharinfo_range[0]);
                                this.NewCharacterInfo.StatMax = int.Parse(newcharinfo_range[1]);
                                break;

                            case "statname":
                                this.NewCharacterInfo.StatNames.AddRange(newcharinfo_value.Split(' '));
                                break;

                            case "race":
                            case "class":
                            case "startingmap":
                                break;

                            default:
                                _Logger.Warning("Unknown newcharinfo: {0}", newcharinfo_line);
                                break;
                        }
                    }
                    break;

                default:
                    _Logger.Warning("Unknown ReplyInfo {0}:\n{1}",
                        e.Request, HexDump.Utils.HexDump(e.Reply));
                    return;
            }

            if (offset < e.Reply.Length)
            {
                _Logger.Warning("Excess Data for ReplyInfo {0}:\n{1}",
                    e.Request, HexDump.Utils.HexDump(e.Reply, offset));
            }

            InfoAvailable?.Invoke(this, new InfoAvailableEventArgs()
            {
                InfoType = e.Request
            });
        }

        public class InfoAvailableEventArgs : EventArgs
        {
            public string InfoType { get; set; }
        }
    }
}

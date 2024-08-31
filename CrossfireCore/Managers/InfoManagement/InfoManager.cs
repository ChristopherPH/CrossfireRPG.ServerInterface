using Common;
using Common.Utility;
using CrossfireRPG.ServerInterface.Definitions;
using CrossfireRPG.ServerInterface.Network;
using CrossfireRPG.ServerInterface.Protocol;
using System;
using System.Collections.Generic;

namespace CrossfireCore.Managers.InfoManagement
{
    public class InfoManager : DataManager
    {
        public InfoManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Handler.ReplyInfo += Handler_ReplyInfo;
        }

        public static Logger Logger { get; } = new Logger(nameof(InfoManager));

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => false;

        public event EventHandler<InfoAvailableEventArgs> InfoAvailable;

        //Server Info
        public string Motd { get; private set; }
        public string News { get; private set; }
        public string Rules { get; private set; }

        public Dictionary<int, SkillInfo> Skills { get; } = new Dictionary<int, SkillInfo>();
        public List<SpellPath> SpellPaths { get; } = new List<SpellPath>();
        public List<KnowledgeInfo> KnowledgeInfos { get; } = new List<KnowledgeInfo>();
        public UInt64[] ExperienceTable { get; private set; }

        public Dictionary<string, RaceClassInfo> Races { get; } = new Dictionary<string, RaceClassInfo>();
        public Dictionary<string, RaceClassInfo> Classes { get; } = new Dictionary<string, RaceClassInfo>();
        public List<StartingMap> StartingMaps { get; } = new List<StartingMap>();
        public NewCharInfo NewCharacterInfo { get; private set; } = new NewCharInfo();
        public ImageInformation ImageInfo { get; private set; } = new ImageInformation();
        public Dictionary<int, ImageSum> ImageSums { get; } = new Dictionary<int, ImageSum>();

        public class KnowledgeInfo
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public int Face { get; set; }
            public int CanAttempt { get; set; }
        }

        public class SkillInfo
        {
            public string Name { get; set; }
            public uint Face { get; set; }
            public string Description { get; set; } = "";
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

        public class ImageInformation
        {
            public int LastFace { get; set; } = 0;
            public int Checksum { get; set; } = 0;
            public List<ImageSet> ImageSets { get; } = new List<ImageSet>();

            public bool GetFaceSetGeometry(int faceset, out int width, out int height)
            {
                foreach (var set in ImageSets)
                {
                    if (faceset == set.ID)
                    {
                        //match pattern of (number)x(number) with allowed whitespace and case
                        //insensitive x
                        var match = System.Text.RegularExpressions.Regex.Match(set.Geometry,
                            @"^\s*(\d+)\s*[xX]\s*(\d+)\s*$");

                        if (!match.Success)
                        {
                            width = 0;
                            height = 0;
                            return false;
                        }

                        //we know we've captured digits so we can assume int.Parse() won't fail here,
                        //hence no error checking
                        width = int.Parse(match.Groups[1].Value);
                        height = int.Parse(match.Groups[2].Value);
                        return true;
                    }
                }

                width = 0;
                height = 0;
                return false;
            }
        }

        public class ImageSet
        {
            public int ID { get; set; }
            public string FileExtension { get; set; }
            public string LongName { get; set; }
            public int Fallback { get; set; }
            public string Geometry { get; set; }
            public string Future { get; set; }
            public string Description { get; set; }

            public override string ToString()
            {
                return $"{ID}: {Geometry} {LongName} {Description}";
            }
        }

        public class ImageSum
        {
            public Int16 Num { get; set; }
            public Int32 Checksum { get; set; }
            public byte FaceSet { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return $"{Num}: {FaceSet} {Name}";
            }
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

        public List<string> GetSpellPaths(uint Path)
        {
            List<string> paths = new List<string>();

            foreach (var i in SpellPaths)
            {
                if ((i.Path & Path) != 0)
                    paths.Add(i.Name);
            }

            return paths;
        }

        public const string InfoTypeMODT = "motd";
        public const string InfoTypeNews = "news";
        public const string InfoTypeRules = "rules";
        public const string InfoTypeSkillInfo = "skill_info";
        public const string InfoTypeSkillExtra = "skill_extra";
        public const string InfoTypeExpTable = "exp_table";
        public const string InfoTypeSpellPaths = "spell_paths";
        public const string InfoTypeKnowledgeInfo = "knowledge_info";
        public const string InfoTypeRaceList = "race_list";
        public const string InfoTypeClassList = "class_list";
        public const string InfoTypeRaceInfo = "race_info";
        public const string InfoTypeClassInfo = "class_info";
        public const string InfoTypeStartingMap = "startingmap";
        public const string InfoTypeNewCharInfo = "newcharinfo";
        public const string InfoTypeImageInfo = "image_info";
        public const string InfoTypeImageSums = "image_sums";
        public const int SkillExtraRequestLevel = 1;

        //Supported Request functions
        public void RequestMOTD()
        {
            Builder.SendRequestInfo(InfoTypeMODT);
        }

        public void RequestNews()
        {
            Builder.SendRequestInfo(InfoTypeNews);
        }

        public void RequestRules()
        {
            Builder.SendRequestInfo(InfoTypeRules);
        }

        public void RequestSkillInfo()
        {
            Builder.SendRequestInfo(InfoTypeSkillInfo, 1);

            //TODO: only request skill extra if its available
            //      with the protocol version
            //      SkillExtra was added without bumping the protocol
            //      so this might be hard to do.
            Builder.SendRequestInfo(InfoTypeSkillExtra, SkillExtraRequestLevel);
        }

        public void RequestExperienceTable()
        {
            Builder.SendRequestInfo(InfoTypeExpTable);
        }

        public void RequestSpellPaths()
        {
            Builder.SendRequestInfo(InfoTypeSpellPaths);
        }

        public void RequestKnowledgeInfo()
        {
            Builder.SendRequestInfo(InfoTypeKnowledgeInfo);
        }

        public void RequestRaces()
        {
            Builder.SendRequestInfo(InfoTypeRaceList);
        }

        public void RequestClasses()
        {
            Builder.SendRequestInfo(InfoTypeClassList);
        }

        public void RequestStartingMap()
        {
            Builder.SendRequestInfo(InfoTypeStartingMap);
        }

        public void RequestNewCharInfo()
        {
            Builder.SendRequestInfo(InfoTypeNewCharInfo);
        }

        public void RequestImageInfo()
        {
            Builder.SendRequestInfo(InfoTypeImageInfo);
        }

        public void RequestImageSums(int start, int stop)
        {
            Builder.SendRequestInfo(string.Format("{0} {1} {2}", InfoTypeImageSums, start, stop));
        }

        protected override void ClearData(bool disconnected)
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
            ImageInfo = new ImageInformation();
            ImageSums.Clear();
        }

        private void Handler_ReplyInfo(object sender, MessageHandler.ReplyInfoEventArgs e)
        {
            int offset = 0;
            var end = e.Reply.Length;

            switch (e.Request)
            {
                case InfoTypeMODT:
                    this.Motd = BufferTokenizer.GetRemainingBytesAsString(e.Reply, ref offset, end);
                    break;

                case InfoTypeNews:
                    this.News = BufferTokenizer.GetRemainingBytesAsString(e.Reply, ref offset, end);
                    break;

                case InfoTypeRules:
                    this.Rules = BufferTokenizer.GetRemainingBytesAsString(e.Reply, ref offset, end);
                    break;

                case InfoTypeSkillInfo:
                    Skills.Clear();
                    while (offset < end)
                    {
                        var SkillSet = BufferTokenizer.GetString(e.Reply, ref offset, end, new byte[] { 0x0A }).Split(':');
                        var SkillID = int.Parse(SkillSet[0]);
                        switch (SkillSet.Length)
                        {
                            case 2: Skills[SkillID] = new SkillInfo() { Name = SkillSet[1].ToTitleCase(), Face = 0 }; break;
                            case 3: Skills[SkillID] = new SkillInfo() { Name = SkillSet[1].ToTitleCase(), Face = UInt32.Parse(SkillSet[2]) }; break;
                            default: throw new MessageParserException("Unhandled skill info count");
                        }
                    }
                    break;

                case InfoTypeSkillExtra:
                    /* HACK: Skill extra was added in 2023 without a protocol
                     * version bump. Metalforge specifically, as well as servers
                     * <= 2023 do not support this replyinfo command.
                     * If the command is not understood, the reply will be the
                     * optional parameters passed in. So check for a match
                     * if the reply length is tiny, to indicate the command
                     * is unsupported.
                     */
                    var requestLevel = SkillExtraRequestLevel.ToString();

                    if (e.Reply.Length == requestLevel.Length)
                    {
                        var tmpSkillExtra = BufferTokenizer.GetRemainingBytesAsString(
                            e.Reply, ref offset, end);

                        if (tmpSkillExtra == requestLevel)
                        {
                            Logger.Warning("Unsupported ReplyInfo {0}:\n{1}",
                                e.Request, HexDump.Utils.HexDump(e.Reply));

                            break;
                        }
                        else
                        {
                            //No match, parse as per normal
                            offset = 0;
                        }
                    }

                    while (offset < end)
                    {
                        var SkillID = BufferTokenizer.GetUInt16(e.Reply, ref offset);
                        if (SkillID == 0)
                            break;

                        var SkillDescriptionLen = BufferTokenizer.GetUInt16(e.Reply, ref offset);
                        var SkillDescription = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, SkillDescriptionLen);

                        if (Skills.TryGetValue(SkillID, out var skill))
                            skill.Description = SkillDescription;
                        else
                            Logger.Warning("Unknown ReplyInfo {0} Skill {1} {2}", e.Request, SkillID, SkillDescription);
                    }
                    break;

                case InfoTypeExpTable:
                    var NumLevels = BufferTokenizer.GetUInt16(e.Reply, ref offset);
                    ExperienceTable = new UInt64[NumLevels];

                    for (int i = 1; i < NumLevels; i++)
                        ExperienceTable[i] = BufferTokenizer.GetUInt64(e.Reply, ref offset);
                    break;

                case InfoTypeSpellPaths:
                    SpellPaths.Clear();
                    while (offset < end)
                    {
                        var SpellPathData = BufferTokenizer.GetString(e.Reply, ref offset, end, new byte[] { 0x0A }).Split(':');

                        var SpellPathID = uint.Parse(SpellPathData[0]);
                        var SpellPathName = SpellPathData[1].ToTitleCase();

                        SpellPaths.Add(new SpellPath() { Path = SpellPathID, Name = SpellPathName });
                    }
                    break;

                case InfoTypeKnowledgeInfo:
                    KnowledgeInfos.Clear();
                    while (offset < end)
                    {
                        var knowledge_info = BufferTokenizer.GetString(e.Reply, ref offset, end, new byte[] { 0x0A }).Split(':');

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

                case InfoTypeRaceList:
                    this.Races.Clear();
                    var race_arches = BufferTokenizer.GetRemainingBytesAsString(e.Reply, ref offset, end).Split('|');
                    foreach (var race in race_arches)
                        if (!string.IsNullOrWhiteSpace(race))
                            Builder.SendRequestInfo(string.Format("{0} {1}", InfoTypeRaceInfo, race));
                    break;

                case InfoTypeRaceInfo:
                    var race_arch = BufferTokenizer.GetString(e.Reply, ref offset, end, BufferTokenizer.NewlineSeperator);

                    this.Races[race_arch] = new RaceClassInfo()
                    {
                        Arch = race_arch
                    };

                    while (offset < end)
                    {
                        var race_info_key = BufferTokenizer.GetString(e.Reply, ref offset, end);

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

                case InfoTypeClassList:
                    this.Classes.Clear();
                    var class_arches = BufferTokenizer.GetRemainingBytesAsString(e.Reply, ref offset, end).Split('|');
                    foreach (var cls in class_arches)
                        if (!string.IsNullOrWhiteSpace(cls))
                            Builder.SendRequestInfo(string.Format("{0} {1}", InfoTypeClassInfo, cls));
                    break;

                case InfoTypeClassInfo:
                    var class_arch = BufferTokenizer.GetString(e.Reply, ref offset, end, BufferTokenizer.NewlineSeperator);

                    this.Classes[class_arch] = new RaceClassInfo()
                    {
                        Arch = class_arch
                    };

                    while (offset < end)
                    {
                        var class_info_key = BufferTokenizer.GetString(e.Reply, ref offset, end);

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

                case InfoTypeStartingMap:
                    StartingMaps.Clear();
                    StartingMap startingMap = null;

                    while (offset < end)
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

                case InfoTypeNewCharInfo:
                    this.NewCharacterInfo = new NewCharInfo();

                    while (offset < end)
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
                                Logger.Warning("Unknown newcharinfo: {0}", newcharinfo_line);
                                break;
                        }
                    }
                    break;

                case InfoTypeImageInfo:

                    var image_last_face = BufferTokenizer.GetString(e.Reply, ref offset, end, new byte[] { 0x0A });
                    var image_info_checksum = BufferTokenizer.GetString(e.Reply, ref offset, end, new byte[] { 0x0A });

                    this.ImageInfo = new ImageInformation();
                    ImageInfo.LastFace = int.Parse(image_last_face);
                    ImageInfo.Checksum = int.Parse(image_info_checksum);

                    while (offset < end)
                    {
                        var image_info = BufferTokenizer.GetString(e.Reply, ref offset, end, new byte[] { 0x0A }).Split(':');

                        var image_info_id = int.Parse(image_info[0]);
                        var image_info_extension = image_info[1];
                        var image_info_longname = image_info[2];
                        var image_info_fallback = int.Parse(image_info[3]);
                        var image_info_geometry = image_info[4];
                        var image_info_future = image_info[5];
                        var image_info_description = image_info[6];

                        this.ImageInfo.ImageSets.Add(new ImageSet()
                        {
                            ID = image_info_id,
                            FileExtension = image_info_extension,
                            LongName = image_info_longname,
                            Fallback = image_info_fallback,
                            Geometry = image_info_geometry,
                            Future = image_info_future,
                            Description = image_info_description,
                        });
                    }
                    break;

                case InfoTypeImageSums:
                    var image_sums_start = int.Parse(BufferTokenizer.GetString(e.Reply, ref offset, end));
                    var image_sums_stop = int.Parse(BufferTokenizer.GetString(e.Reply, ref offset, end));
                    var image_sums_cur = image_sums_start;

                    while (offset < end)
                    {
                        var image_sums_num = BufferTokenizer.GetInt16(e.Reply, ref offset);

                        if (image_sums_cur != image_sums_num)
                            Logger.Warning("image_sums mismatch: cur num {0} != num {1}",
                                image_sums_cur, image_sums_num);

                        var image_sums_ckcum = BufferTokenizer.GetInt32(e.Reply, ref offset);
                        var image_sums_faceset = BufferTokenizer.GetByte(e.Reply, ref offset);
                        var image_sums_namelen = BufferTokenizer.GetByte(e.Reply, ref offset);
                        var image_sums_name = BufferTokenizer.GetBytesAsString(e.Reply, ref offset, image_sums_namelen - 1);
                        BufferTokenizer.GetByte(e.Reply, ref offset); //skip null terminator

                        ImageSums[image_sums_cur] = new ImageSum()
                        {
                            Num = image_sums_num,
                            Checksum = image_sums_ckcum,
                            FaceSet = image_sums_faceset,
                            Name = image_sums_name,
                        };

                        image_sums_cur++;
                    }

                    if (image_sums_cur - 1 != image_sums_stop)
                        Logger.Warning("image_sums mismatch: cur num {0} != stop {1}",
                            image_sums_cur - 1, image_sums_stop);
                    break;

                default:
                    Logger.Warning("Unknown ReplyInfo {0}:\n{1}",
                        e.Request, HexDump.Utils.HexDump(e.Reply));
                    return;
            }

            if (offset < end)
            {
                Logger.Warning("Excess Data for ReplyInfo {0}:\n{1}",
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

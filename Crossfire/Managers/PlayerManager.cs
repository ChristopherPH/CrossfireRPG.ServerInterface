using Common;
using CrossfireCore;
using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.Managers
{
    public class PlayerManager : DataManager<Player>
    {
        public PlayerManager(SocketConnection Connection, MessageBuilder Builder, MessageParser Parser)
            : base(Connection, Builder, Parser)
        {
            Parser.Player += Parser_Player;
            Parser.UpdateItem += Parser_UpdateItem;
            Parser.Stats += Parser_Stats;
            Parser.BeginStats += Parser_BeginStats;
            Parser.EndStats += Parser_EndStats;
        }

        static Logger _Logger = new Logger(nameof(PlayerManager));
        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => false;
        public override ModificationTypes SupportedModificationTypes => ModificationTypes.Updated |
            ModificationTypes.MultiCommandStart | ModificationTypes.MultiCommandEnd;

        public Player Player { get; private set; } = new Player();

        protected override void ClearData()
        {
            Player = new Player();
            OnDataChanged(ModificationTypes.Updated, Player, null);
        }

        private void Parser_Player(object sender, MessageParser.PlayerEventArgs e)
        {
            _Logger.Info("New Player: Tag={0} Name='{1}' Face={2} Weight={3}", e.tag, e.PlayerName, e.face, e.weight);

            //Technically the 'Player' command creates a new player or clears an existing player.
            //However, the server maintains the player properties and stats for the life of the connection,
            //and only sends updates if the data changes, so we can't clear data on a new player, only update it
            if (Player.PlayerTag != e.tag)
            {
                Player.PlayerTag = e.tag;
                base.OnPropertyChanged(Player, nameof(Managers.Player.PlayerTag));
            }

            if (Player.Name != e.PlayerName)
            {
                Player.Name = e.PlayerName;
                base.OnPropertyChanged(Player, nameof(Managers.Player.Name));
            }

            if (Player.Face != e.face)
            {
                Player.Face = e.face;
                base.OnPropertyChanged(Player, nameof(Managers.Player.Face));
            }

            if (Player.RawWeight != e.weight)
            {
                Player.RawWeight = e.weight;
                base.OnPropertyChanged(Player, nameof(Managers.Player.Weight));
            }
        }

        private void Parser_UpdateItem(object sender, MessageParser.UpdateItemEventArgs e)
        {
            //UpdateItem will be called with the PlayerTag as the ObjectTag to update certain properties
            if (!Player.ValidPlayer || (e.ObjectTag != Player.PlayerTag))
                return;

            _Logger.Info("Update Player: {0} => {1}{2}", e.UpdateType, e.UpdateValue, e.UpdateString);

            switch (e.UpdateType)
            {
                case NewClient.UpdateTypes.Name:
                    if (Player.Name != e.UpdateString)
                    {
                        Player.Name = e.UpdateString;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Name));
                    }
                    break;

                case NewClient.UpdateTypes.Face:
                    if (Player.Face != e.UpdateValue)
                    {
                        Player.Face = (UInt32)e.UpdateValue;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Face));
                    }
                    break;

                case NewClient.UpdateTypes.Weight:
                    if (Player.RawWeight != e.UpdateValue)
                    {
                        Player.RawWeight = (UInt32)e.UpdateValue;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Weight));
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private void Parser_Stats(object sender, MessageParser.StatEventArgs e)
        {
            //Technically the 'Player' command creates a new player or clears an existing player.
            //However, the server maintains the player properties and stats for the life of the connection,
            //and only sends updates if the data changes, so we can't clear data on a new player, only update it
            _Logger.Info("Player Stats: {0} => {1}", e.Stat, e.Value);

            UInt32 u32;
            UInt64 u64;

            switch (e.Stat)
            {
                case NewClient.CharacterStats.Hp:
                    if (UInt32.TryParse(e.Value, out u32) && (Player.Health != u32))
                    {
                        Player.Health = u32;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Health));
                    }
                    break;

                case NewClient.CharacterStats.MaxHp:
                    if (UInt32.TryParse(e.Value, out u32) && (Player.MaxHealth != u32))
                    {
                        Player.MaxHealth = u32;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.MaxHealth));
                    }
                    break;

                case NewClient.CharacterStats.Sp:
                    if (UInt32.TryParse(e.Value, out u32) && (Player.Mana != u32))
                    {
                        Player.Mana = u32;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Mana));
                    }
                    break;

                case NewClient.CharacterStats.MaxSp:
                    if (UInt32.TryParse(e.Value, out u32) && (Player.MaxMana != u32))
                    {
                        Player.MaxMana = u32;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.MaxMana));
                    }
                    break;

                case NewClient.CharacterStats.Grace:
                    if (UInt32.TryParse(e.Value, out u32) && (Player.Grace != u32))
                    {
                        Player.Grace = u32;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Grace));
                    }
                    break;

                case NewClient.CharacterStats.MaxGrace:
                    if (UInt32.TryParse(e.Value, out u32) && (Player.MaxGrace != u32))
                    {
                        Player.MaxGrace = u32;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.MaxGrace));
                    }
                    break;

                case NewClient.CharacterStats.Exp:
                case NewClient.CharacterStats.Exp64:
                    if (UInt64.TryParse(e.Value, out u64) && (Player.Experience != u64))
                    {
                        Player.Experience = u64;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Experience));
                    }
                    break;

                case NewClient.CharacterStats.Level:
                    if (UInt32.TryParse(e.Value, out u32) && (Player.Level != u32))
                    {
                        Player.Level = u32;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Level));
                    }
                    break;

                case NewClient.CharacterStats.Food:
                    if (UInt32.TryParse(e.Value, out u32) && (Player.Food != u32))
                    {
                        Player.Food = u32;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Food));
                    }
                    break;

                case NewClient.CharacterStats.Title:
                    var tmpTitle = e.Value;
                    if (tmpTitle.StartsWith("Player: "))
                        tmpTitle = tmpTitle.Remove(0, "Player: ".Length);

                    if (Player.Title != tmpTitle)
                    {
                        Player.Title = tmpTitle;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Title));
                    }
                    break;

                case NewClient.CharacterStats.Range:
                    var tmpRange = e.Value;
                    if (tmpRange.StartsWith("Range: "))
                        tmpRange = tmpRange.Remove(0, "Range: ".Length);
                    if (tmpRange.StartsWith("Skill: "))
                        tmpRange = tmpRange.Remove(0, "Skill: ".Length);

                    if (Player.Range != tmpRange)
                    {
                        Player.Range = tmpRange;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Range));
                    }
                    break;

                case NewClient.CharacterStats.WeightLim:
                    if (UInt32.TryParse(e.Value, out u32) && (Player.RawWeightLimit != u32))
                    {
                        Player.RawWeightLimit = u32;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.WeightLimit));
                    }
                    break;

                case NewClient.CharacterStats.Flags:
                    if (Enum.TryParse<NewClient.StatFlags>(e.Value, out var statFlags) && (Player.StatFlags != statFlags))
                    {
                        Player.StatFlags = statFlags;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.StatFlags));
                    }
                    break;

                case NewClient.CharacterStats.CharacterFlags:
                    if (Enum.TryParse<NewClient.CharacterFlags>(e.Value, out var charFlags) && (Player.CharacterFlags != charFlags))
                    {
                        Player.CharacterFlags = charFlags;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.CharacterFlags));
                    }
                    break;

                default:
                    if (!Player.Stats.ContainsKey(e.Stat) || (Player.Stats[e.Stat] != e.Value))
                    {
                        Player.Stats[e.Stat] = e.Value;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Stats) + "|" + e.Stat.ToString());
                    }
                    break;
            }
        }

        private void Parser_BeginStats(object sender, EventArgs e)
        {
            StartMultiCommand();
        }

        private void Parser_EndStats(object sender, EventArgs e)
        {
            EndMultiCommand();
        }
    }

    public class Player
    {
        /// <summary>
        /// Tag of 0 indicates no active player
        /// </summary>
        public UInt32 PlayerTag { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public UInt32 Face { get; set; } = 0;

        public UInt32 RawWeight { get; set; } = 0;
        public float Weight => RawWeight / 1000; //TODO: use constant
        public UInt32 RawWeightLimit { get; set; } = 0;
        public float WeightLimit => RawWeightLimit / 1000; //TODO: use constant

        public UInt32 Health { get; set; } = 0;
        public UInt32 MaxHealth { get; set; } = 0;
        public UInt32 Mana { get; set; } = 0;
        public UInt32 MaxMana { get; set; } = 0;
        public UInt32 Grace { get; set; } = 0;
        public UInt32 MaxGrace { get; set; } = 0;
        public UInt32 Food { get; set; } = 0;
        public UInt64 Experience { get; set; } = 0;
        public UInt32 Level { get; set; } = 0;
        public string Range { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public NewClient.StatFlags StatFlags { get; set; } = 0;
        public NewClient.CharacterFlags CharacterFlags { get; set; } = 0;

        public Dictionary<NewClient.CharacterStats, string> Stats { get; } =
            new Dictionary<NewClient.CharacterStats, string>();

        public bool ValidPlayer => PlayerTag != 0;

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Name) ? PlayerTag.ToString() : Name;
        }
    }
}

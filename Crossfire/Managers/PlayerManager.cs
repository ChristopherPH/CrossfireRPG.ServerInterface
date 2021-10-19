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
        }

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => false;
        public override ModificationTypes SupportedModificationTypes => ModificationTypes.Updated;

        public Player Player { get; private set; } = new Player();

        protected override void ClearData()
        {
            if (Player.ValidPlayer)
            {
                Player = new Player();
                OnDataChanged(ModificationTypes.Updated, Player, null);
            }
        }

        private void Parser_Player(object sender, MessageParserBase.PlayerEventArgs e)
        {
            ClearData();

            //Tag of 0 indicates no player
            if (e.tag == 0)
                return;

            //create new player
            Player.PlayerTag = e.tag;
            Player.Name = e.PlayerName;
            Player.Face = e.face;
            Player.RawWeight = e.weight;

            base.OnDataChanged(ModificationTypes.Updated, Player, new string[] {
                nameof(Managers.Player.PlayerTag), nameof(Managers.Player.Name), nameof(Managers.Player.Face),
                nameof(Managers.Player.Weight)});
        }

        private void Parser_UpdateItem(object sender, MessageParserBase.UpdateItemEventArgs e)
        {
            //UpdateItem will be called with the playertag to update certain properties
            if (!Player.ValidPlayer || (e.ObjectTag != Player.PlayerTag))
                return;

            switch (e.UpdateType)
            {
                case CrossfireCore.NewClient.UpdateTypes.Name:
                    if (Player.Name != e.UpdateString)
                    {
                        Player.Name = e.UpdateString;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Name));
                    }
                    break;

                case CrossfireCore.NewClient.UpdateTypes.Face:
                    if (Player.Face != e.UpdateValue)
                    {
                        Player.Face = (UInt32)e.UpdateValue;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Face));
                    }
                    break;

                case CrossfireCore.NewClient.UpdateTypes.Weight:
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

        private void Parser_Stats(object sender, MessageParserBase.StatEventArgs e)
        {
            if (!Player.ValidPlayer)
                return;

            UInt32 u32;
            UInt64 u64;

            switch (e.Stat)
            {
                case CrossfireCore.NewClient.CharacterStats.Hp:
                    if (UInt32.TryParse(e.Value, out u32) && (Player.Health != u32))
                    {
                        Player.Health = u32;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Health));
                    }
                    break;

                case CrossfireCore.NewClient.CharacterStats.MaxHp:
                    if (UInt32.TryParse(e.Value, out u32) && (Player.MaxHealth != u32))
                    {
                        Player.MaxHealth = u32;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.MaxHealth));
                    }
                    break;

                case CrossfireCore.NewClient.CharacterStats.Sp:
                    if (UInt32.TryParse(e.Value, out u32) && (Player.Mana != u32))
                    {
                        Player.Mana = u32;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Mana));
                    }
                    break;

                case CrossfireCore.NewClient.CharacterStats.MaxSp:
                    if (UInt32.TryParse(e.Value, out u32) && (Player.MaxMana != u32))
                    {
                        Player.MaxMana = u32;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.MaxMana));
                    }
                    break;

                case CrossfireCore.NewClient.CharacterStats.Grace:
                    if (UInt32.TryParse(e.Value, out u32) && (Player.Grace != u32))
                    {
                        Player.Grace = u32;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Grace));
                    }
                    break;

                case CrossfireCore.NewClient.CharacterStats.MaxGrace:
                    if (UInt32.TryParse(e.Value, out u32) && (Player.MaxGrace != u32))
                    {
                        Player.MaxGrace = u32;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.MaxGrace));
                    }
                    break;

                case CrossfireCore.NewClient.CharacterStats.Exp:
                case CrossfireCore.NewClient.CharacterStats.Exp64:
                    if (UInt64.TryParse(e.Value, out u64) && (Player.Experience != u64))
                    {
                        Player.Experience = u64;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Experience));
                    }
                    break;

                case CrossfireCore.NewClient.CharacterStats.Level:
                    if (UInt32.TryParse(e.Value, out u32) && (Player.Level != u32))
                    {
                        Player.Level = u32;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Level));
                    }
                    break;

                case CrossfireCore.NewClient.CharacterStats.Food:
                    if (UInt32.TryParse(e.Value, out u32) && (Player.Food != u32))
                    {
                        Player.Food = u32;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Food));
                    }
                    break;

                case CrossfireCore.NewClient.CharacterStats.Title:
                    if (Player.Title != e.Value)
                    {
                        Player.Title = e.Value;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Title));
                    }
                    break;

                case CrossfireCore.NewClient.CharacterStats.Range:
                    if (Player.Range != e.Value)
                    {
                        Player.Range = e.Value;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.Range));
                    }
                    break;

                case CrossfireCore.NewClient.CharacterStats.WeightLim:
                    if (UInt32.TryParse(e.Value, out u32) && (Player.RawWeightLimit != u32))
                    {
                        Player.RawWeightLimit = u32;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.WeightLimit));
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
    }

    public class Player
    {
        public UInt32 PlayerTag { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public UInt32 Face { get; set; } = 0;

        public UInt32 RawWeight { get; set; } = 0;
        public float Weight => RawWeight / 1000;
        public UInt32 RawWeightLimit { get; set; } = 0;
        public float WeightLimit => RawWeightLimit / 1000;

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

        public Dictionary<CrossfireCore.NewClient.CharacterStats, string> Stats { get; } =
            new Dictionary<CrossfireCore.NewClient.CharacterStats, string>();

        public bool ValidPlayer => PlayerTag != 0;

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Name) ? PlayerTag.ToString() : Name;
        }
    }
}

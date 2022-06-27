using Common;
using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossfireCore.Managers
{
    public class PlayerManager : DataManager<Player>
    {
        public PlayerManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Handler.Player += Handler_Player;
            Handler.UpdateItem += Handler_UpdateItem;
            Handler.Stats += Handler_Stats;
            Handler.BeginStats += Handler_BeginStats;
            Handler.EndStats += Handler_EndStats;
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

        private void Handler_Player(object sender, MessageHandler.PlayerEventArgs e)
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

            if (Player.Weight != e.weight)
            {
                Player.Weight = e.weight;
                base.OnPropertyChanged(Player, nameof(Managers.Player.Weight));
            }
        }

        private void Handler_UpdateItem(object sender, MessageHandler.UpdateItemEventArgs e)
        {
            //UpdateItem will be called with the PlayerTag as the ObjectTag to update certain properties
            if (!Player.ValidPlayer || (e.ObjectTag != Player.PlayerTag))
                return;

            _Logger.Info("Update Player: {0} => {1}{2}{3}{4}{5}", e.UpdateType,
                e.UpdateValueUInt8, e.UpdateValueUInt16, e.UpdateValueUInt32,
                e.UpdateValueFloat, e.UpdateString);

            switch (e.UpdateType)
            {
                case NewClient.UpdateTypes.Name:
                    if (e.DataType == MessageHandler.UpdateItemEventArgs.UpdateDataTypes.String)
                    {
                        if (Player.Name != e.UpdateString)
                        {
                            Player.Name = e.UpdateString;
                            base.OnPropertyChanged(Player, nameof(Managers.Player.Name));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.UpdateTypes.Name has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.UpdateTypes.Face:
                    if (e.DataType == MessageHandler.UpdateItemEventArgs.UpdateDataTypes.UInt32)
                    {
                        if (Player.Face != e.UpdateValueUInt32)
                        {
                            Player.Face = e.UpdateValueUInt32;
                            base.OnPropertyChanged(Player, nameof(Managers.Player.Face));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.UpdateTypes.Face has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.UpdateTypes.Weight:
                    if (e.DataType == MessageHandler.UpdateItemEventArgs.UpdateDataTypes.Float)
                    {
                        if (Player.Weight != e.UpdateValueFloat)
                        {
                            Player.Weight = e.UpdateValueFloat;
                            base.OnPropertyChanged(Player, nameof(Managers.Player.Weight));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.UpdateTypes.Weight has wrong datatype: {0}", e.DataType);
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private void Handler_Stats(object sender, MessageHandler.StatEventArgs e)
        {
            //Technically the 'Player' command creates a new player or clears an existing player.
            //However, the server maintains the player properties and stats for the life of the connection,
            //and only sends updates if the data changes, so we can't clear data on a new player, only update it
            _Logger.Info("Player Stats: {0} => {1}", e.Stat, e.ValueAsString);

            switch (e.Stat)
            {
                case NewClient.CharacterStats.Hp:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.UInt16)
                    {
                        if (Player.Health != e.ValueUInt16)
                        {
                            Player.Health = e.ValueUInt16;
                            base.OnPropertyChanged(Player, nameof(Managers.Player.Health));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Health has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.MaxHp:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.MaxHealth != e.ValueInt16)
                        {
                            Player.MaxHealth = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Managers.Player.MaxHealth));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.MaxHealth has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Sp:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Mana != e.ValueInt16)
                        {
                            Player.Mana = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Managers.Player.Mana));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Mana has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.MaxSp:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.MaxMana != e.ValueInt16)
                        {
                            Player.MaxMana = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Managers.Player.MaxMana));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.MaxMana has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Grace:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Grace != e.ValueInt16)
                        {
                            Player.Grace = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Managers.Player.Grace));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Grace has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.MaxGrace:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.MaxGrace != e.ValueInt16)
                        {
                            Player.MaxGrace = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Managers.Player.MaxGrace));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.MaxGrace has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Exp:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Experience != (UInt64)e.ValueInt16)
                        {
                            Player.Experience = (UInt64)e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Managers.Player.Experience));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Experience has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Exp64:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.UInt64)
                    {
                        if (Player.Experience != e.ValueUInt64)
                        {
                            Player.Experience = e.ValueUInt64;
                            base.OnPropertyChanged(Player, nameof(Managers.Player.Experience));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Experience has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Level:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Level != e.ValueInt16)
                        {
                            Player.Level = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Managers.Player.Level));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Level has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Food:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Food != e.ValueInt16)
                        {
                            Player.Food = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Managers.Player.Food));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Food has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Title:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.String)
                    {
                        var tmpTitle = e.ValueString;
                        if (tmpTitle.StartsWith("Player: "))
                            tmpTitle = tmpTitle.Remove(0, "Player: ".Length);

                        if (Player.Title != tmpTitle)
                        {
                            Player.Title = tmpTitle;
                            base.OnPropertyChanged(Player, nameof(Managers.Player.Title));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Title has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Range:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.String)
                    {
                        var tmpRange = e.ValueString;
                        if (tmpRange.StartsWith("Range: "))
                            tmpRange = tmpRange.Remove(0, "Range: ".Length);
                        if (tmpRange.StartsWith("Skill: "))
                            tmpRange = tmpRange.Remove(0, "Skill: ".Length);

                        if (Player.Range != tmpRange)
                        {
                            Player.Range = tmpRange;
                            base.OnPropertyChanged(Player, nameof(Managers.Player.Range));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Range has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.WeightLim:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Float)
                    {
                        if (Player.WeightLimit != e.ValueFloat)
                        {
                            Player.WeightLimit = e.ValueFloat;
                            base.OnPropertyChanged(Player, nameof(Managers.Player.WeightLimit));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.WeightLimit has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Flags:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.StatFlags != (NewClient.StatFlags)e.ValueInt16)
                        {
                            Player.StatFlags = (NewClient.StatFlags)e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Managers.Player.StatFlags));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.StatFlags has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.CharacterFlags:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.UInt32)
                    {
                        if (Player.CharacterFlags != (NewClient.CharacterFlags)e.ValueUInt32)
                        {
                            Player.CharacterFlags = (NewClient.CharacterFlags)e.ValueUInt32;
                            base.OnPropertyChanged(Player, nameof(Managers.Player.CharacterFlags));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.CharacterFlags has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Str:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Strength != e.ValueInt16)
                        {
                            Player.Strength = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.Strength));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Strength has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Int:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Intellegence != e.ValueInt16)
                        {
                            Player.Intellegence = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.Intellegence));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Intellegence has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Pow:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Power != e.ValueInt16)
                        {
                            Player.Power = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.Power));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Power has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Wis:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Wisdom != e.ValueInt16)
                        {
                            Player.Wisdom = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.Wisdom));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Wisdom has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Dex:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Dexterity != e.ValueInt16)
                        {
                            Player.Dexterity = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.Dexterity));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Dexterity has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Con:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Constitution != e.ValueInt16)
                        {
                            Player.Constitution = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.Constitution));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Constitution has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Cha:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Charisma != e.ValueInt16)
                        {
                            Player.Charisma = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.Charisma));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Charisma has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.RaceStr:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.RaceStrength != e.ValueInt16)
                        {
                            Player.RaceStrength = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.RaceStrength));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.RaceStrength has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.RaceInt:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.RaceIntellegence != e.ValueInt16)
                        {
                            Player.RaceIntellegence = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.RaceIntellegence));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.RaceIntellegence has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.RacePow:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.RacePower != e.ValueInt16)
                        {
                            Player.RacePower = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.RacePower));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.RacePower has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.RaceWis:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.RaceWisdom != e.ValueInt16)
                        {
                            Player.RaceWisdom = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.RaceWisdom));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.RaceWisdom has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.RaceDex:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.RaceDexterity != e.ValueInt16)
                        {
                            Player.RaceDexterity = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.RaceDexterity));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.RaceDexterity has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.RaceCon:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.RaceConstitution != e.ValueInt16)
                        {
                            Player.RaceConstitution = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.RaceConstitution));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.RaceConstitution has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.RaceCha:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.RaceCharisma != e.ValueInt16)
                        {
                            Player.RaceCharisma = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.RaceCharisma));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.RaceCharisma has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.BaseStr:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.BaseStrength != e.ValueInt16)
                        {
                            Player.BaseStrength = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.BaseStrength));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.BaseStrength has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.BaseInt:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.BaseIntellegence != e.ValueInt16)
                        {
                            Player.BaseIntellegence = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.BaseIntellegence));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.BaseIntellegence has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.BasePow:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.BasePower != e.ValueInt16)
                        {
                            Player.BasePower = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.BasePower));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.BasePower has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.BaseWis:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.BaseWisdom != e.ValueInt16)
                        {
                            Player.BaseWisdom = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.BaseWisdom));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.BaseWisdom has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.BaseDex:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.BaseDexterity != e.ValueInt16)
                        {
                            Player.BaseDexterity = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.BaseDexterity));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.BaseDexterity has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.BaseCon:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.BaseConstitution != e.ValueInt16)
                        {
                            Player.BaseConstitution = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.BaseConstitution));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.BaseConstitution has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.BaseCha:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.BaseCharisma != e.ValueInt16)
                        {
                            Player.BaseCharisma = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.BaseCharisma));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.BaseCharisma has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.AppliedStr:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.AppliedStrength != e.ValueInt16)
                        {
                            Player.AppliedStrength = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.AppliedStrength));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.AppliedStrength has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.AppliedInt:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.AppliedIntellegence != e.ValueInt16)
                        {
                            Player.AppliedIntellegence = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.AppliedIntellegence));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.AppliedIntellegence has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.AppliedPow:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.AppliedPower != e.ValueInt16)
                        {
                            Player.AppliedPower = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.AppliedPower));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.AppliedPower has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.AppliedWis:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.AppliedWisdom != e.ValueInt16)
                        {
                            Player.AppliedWisdom = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.AppliedWisdom));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.AppliedWisdom has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.AppliedDex:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.AppliedDexterity != e.ValueInt16)
                        {
                            Player.AppliedDexterity = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.AppliedDexterity));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.AppliedDexterity has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.AppliedCon:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.AppliedContitution != e.ValueInt16)
                        {
                            Player.AppliedContitution = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.AppliedContitution));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.AppliedContitution has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.AppliedCha:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.AppliedCharisma != e.ValueInt16)
                        {
                            Player.AppliedCharisma = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.AppliedCharisma));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.AppliedCharisma has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Speed:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Float)
                    {
                        if (Player.Speed != e.ValueFloat)
                        {
                            Player.Speed = e.ValueFloat;
                            base.OnPropertyChanged(Player, nameof(Player.Speed));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Speed has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.GodName:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.String)
                    {
                        if (Player.GodName != e.ValueString)
                        {
                            Player.GodName = e.ValueString;
                            base.OnPropertyChanged(Player, nameof(Player.GodName));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.GodName has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Overload:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Float)
                    {
                        if (Player.Overload != e.ValueFloat)
                        {
                            Player.Overload = e.ValueFloat;
                            base.OnPropertyChanged(Player, nameof(Player.Overload));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Overload has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.GolemHp:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.GolemHealth != e.ValueInt16)
                        {
                            Player.GolemHealth = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.GolemHealth));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.GolemHealth has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.GolemMaxHp:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.GolemMaxHealth != e.ValueInt16)
                        {
                            Player.GolemMaxHealth = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.GolemMaxHealth));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.GolemMaxHealth has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResPhys:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistPhysical != e.ValueInt16)
                        {
                            Player.ResistPhysical = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.ResistPhysical));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.ResistPhysical has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResMag:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistMagic != e.ValueInt16)
                        {
                            Player.ResistMagic = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.ResistMagic));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.ResistMagic has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResFire:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistFire != e.ValueInt16)
                        {
                            Player.ResistFire = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.ResistFire));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.ResistFire has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResElec:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistElectricity != e.ValueInt16)
                        {
                            Player.ResistElectricity = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.ResistElectricity));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.ResistElectricity has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResCold:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistCold != e.ValueInt16)
                        {
                            Player.ResistCold = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.ResistCold));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.ResistCold has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResConf:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistConfusion != e.ValueInt16)
                        {
                            Player.ResistConfusion = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.ResistConfusion));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.ResistConfusion has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResAcid:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistAcid != e.ValueInt16)
                        {
                            Player.ResistAcid = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.ResistAcid));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.ResistAcid has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResDrain:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistDrain != e.ValueInt16)
                        {
                            Player.ResistDrain = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.ResistDrain));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.ResistDrain has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResGhosthit:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistGhostHit != e.ValueInt16)
                        {
                            Player.ResistGhostHit = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.ResistGhostHit));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.ResistGhostHit has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResPoison:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistPoison != e.ValueInt16)
                        {
                            Player.ResistPoison = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.ResistPoison));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.ResistPoison has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResSlow:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistSlow != e.ValueInt16)
                        {
                            Player.ResistSlow = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.ResistSlow));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.ResistSlow has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResPara:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistParalysis != e.ValueInt16)
                        {
                            Player.ResistParalysis = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.ResistParalysis));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.ResistParalysis has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.TurnUndead:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.TurnUndead != e.ValueInt16)
                        {
                            Player.TurnUndead = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.TurnUndead));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.TurnUndead has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResFear:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistFear != e.ValueInt16)
                        {
                            Player.ResistFear = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.ResistFear));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.ResistFear has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResDeplete:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistDeplete != e.ValueInt16)
                        {
                            Player.ResistDeplete = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.ResistDeplete));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.ResistDeplete has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResDeath:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistDeath != e.ValueInt16)
                        {
                            Player.ResistDeath = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.ResistDeath));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.ResistDeath has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResHolyword:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistHolyWord != e.ValueInt16)
                        {
                            Player.ResistHolyWord = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.ResistHolyWord));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.ResistHolyWord has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResBlind:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistBlindness != e.ValueInt16)
                        {
                            Player.ResistBlindness = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.ResistBlindness));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.ResistBlindness has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Wc:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.WeaponClass != e.ValueInt16)
                        {
                            Player.WeaponClass = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.WeaponClass));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.WeaponClass has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.WeapSp:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Float)
                    {
                        if (Player.WeaponSpeed != e.ValueFloat)
                        {
                            Player.WeaponSpeed = e.ValueFloat;
                            base.OnPropertyChanged(Player, nameof(Player.WeaponSpeed));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.WeaponSpeed has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Dam:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Damage != e.ValueInt16)
                        {
                            Player.Damage = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.Damage));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Damage has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Ac:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ArmourClass != e.ValueInt16)
                        {
                            Player.ArmourClass = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.ArmourClass));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.ArmourClass has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Armour:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Armour != e.ValueInt16)
                        {
                            Player.Armour = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.Armour));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.Armour has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ItemPower:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ItemPower != e.ValueInt16)
                        {
                            Player.ItemPower = e.ValueInt16;
                            base.OnPropertyChanged(Player, nameof(Player.ItemPower));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.ItemPower has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.SpellAttune:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.UInt32)
                    {
                        if (Player.SpellAttune != e.ValueUInt32)
                        {
                            Player.SpellAttune = e.ValueUInt32;
                            base.OnPropertyChanged(Player, nameof(Player.SpellAttune));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.SpellAttune has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.SpellRepel:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.UInt32)
                    {
                        if (Player.SpellRepel != e.ValueUInt32)
                        {
                            Player.SpellRepel = e.ValueUInt32;
                            base.OnPropertyChanged(Player, nameof(Player.SpellRepel));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.SpellRepel has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.SpellDeny:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.UInt32)
                    {
                        if (Player.SpellDeny != e.ValueUInt32)
                        {
                            Player.SpellDeny = e.ValueUInt32;
                            base.OnPropertyChanged(Player, nameof(Player.SpellDeny));
                        }
                    }
                    else
                    {
                        _Logger.Warning("Player.SpellDeny has wrong datatype: {0}", e.DataType);
                    }
                    break;

                //the leftover stats are actually skills
                default:
                    _Logger.Warning("Player has unnapped stat: {0}", e.Stat);

                    if (!Player.UnmappedStats.ContainsKey(e.Stat) || (Player.UnmappedStats[e.Stat] != e.ValueAsString))
                    {
                        Player.UnmappedStats[e.Stat] = e.ValueAsString;
                        base.OnPropertyChanged(Player, nameof(Managers.Player.UnmappedStats) + "|" + e.Stat.ToString());
                    }
                    break;
            }
        }

        private void Handler_BeginStats(object sender, EventArgs e)
        {
            StartMultiCommand();
        }

        private void Handler_EndStats(object sender, EventArgs e)
        {
            EndMultiCommand();
        }
    }
}

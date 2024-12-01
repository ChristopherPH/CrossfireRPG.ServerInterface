/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using Common.Logging;
using CrossfireRPG.ServerInterface.Definitions;
using CrossfireRPG.ServerInterface.Network;
using CrossfireRPG.ServerInterface.Protocol;
using System;

namespace CrossfireRPG.ServerInterface.Managers.PlayerManagement
{
    public class PlayerManager : DataObjectManager<Player>
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

        public static Logger Logger { get; } = new Logger(nameof(PlayerManager));
        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => false;
        public override DataModificationTypes SupportedModificationTypes => DataModificationTypes.Updated |
            DataModificationTypes.MultiCommandStart | DataModificationTypes.MultiCommandEnd;

        public Player Player { get; private set; } = new Player();

        protected override void ClearData(bool disconnected)
        {
            Player = new Player();
            OnDataChanged(DataModificationTypes.Updated, Player, null);
        }

        private void Handler_Player(object sender, MessageHandler.PlayerEventArgs e)
        {
            Logger.Debug("Handler_Player(): Tag={0} Name='{1}' Face={2} Weight={3}",
                e.tag, e.PlayerName, e.face, e.weight);

            //Technically the 'Player' command creates a new player or clears an existing player.
            //However, the server maintains the player properties and stats for the life of the connection,
            //and only sends updates if the data changes, so we can't clear data on a new player, only update it
            if (Player.PlayerTag != e.tag)
            {
                Player.PlayerTag = e.tag;
                Logger.Info("Player.PlayerTag = {0}", Player.PlayerTag);
                base.OnPropertyChanged(Player, nameof(Player.PlayerTag));
            }

            if (Player.Name != e.PlayerName)
            {
                Player.Name = e.PlayerName;
                Logger.Info("Player.Name = {0}", Player.Name);
                base.OnPropertyChanged(Player, nameof(Player.Name));
            }

            if (Player.Face != e.face)
            {
                Player.Face = e.face;
                Logger.Info("Player.Face = {0}", Player.Face);
                base.OnPropertyChanged(Player, nameof(Player.Face));
            }

            if (Player.Weight != e.weight)
            {
                Player.Weight = e.weight;
                Logger.Info("Player.Weight = {0}", Player.Weight);
                base.OnPropertyChanged(Player, nameof(Player.Weight));
            }
        }

        private void Handler_UpdateItem(object sender, MessageHandler.UpdateItemEventArgs e)
        {
            //UpdateItem will be called with the PlayerTag as the ObjectTag to update certain properties
            if (!Player.ValidPlayer || (e.ObjectTag != Player.PlayerTag))
                return;

            Logger.Debug("Handler_UpdateItem(): {0} => {1}", e.UpdateType, e.ValueAsString);

            switch (e.UpdateType)
            {
                case NewClient.UpdateTypes.Name:
                    if (e.DataType == MessageHandler.UpdateItemEventArgs.UpdateDataTypes.String)
                    {
                        if (Player.Name != e.UpdateString)
                        {
                            Player.Name = e.UpdateString;
                            Logger.Info("Player.Name = {0}", Player.Name);
                            base.OnPropertyChanged(Player, nameof(Player.Name));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.UpdateTypes.Name has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.UpdateTypes.Face:
                    if (e.DataType == MessageHandler.UpdateItemEventArgs.UpdateDataTypes.UInt32)
                    {
                        if (Player.Face != e.UpdateValueUInt32)
                        {
                            Player.Face = e.UpdateValueUInt32;
                            Logger.Info("Player.Face = {0}", Player.Face);
                            base.OnPropertyChanged(Player, nameof(Player.Face));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.UpdateTypes.Face has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.UpdateTypes.Weight:
                    if (e.DataType == MessageHandler.UpdateItemEventArgs.UpdateDataTypes.Float)
                    {
                        if (Player.Weight != e.UpdateValueFloat)
                        {
                            Player.Weight = e.UpdateValueFloat;
                            Logger.Info("Player.Weight = {0}", Player.Weight);
                            base.OnPropertyChanged(Player, nameof(Player.Weight));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.UpdateTypes.Weight has wrong datatype: {0}", e.DataType);
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
            Logger.Debug("Handler_Stats(): {0} => {1}", e.Stat, e.ValueAsString);

            switch (e.Stat)
            {
                case NewClient.CharacterStats.Hp:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.UInt16)
                    {
                        if (Player.Health != e.ValueUInt16)
                        {
                            Player.Health = e.ValueUInt16;
                            Logger.Info("Player.Health = {0}", Player.Health);
                            base.OnPropertyChanged(Player, nameof(Player.Health));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Health has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.MaxHp:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.MaxHealth != e.ValueInt16)
                        {
                            Player.MaxHealth = e.ValueInt16;
                            Logger.Info("Player.MaxHealth = {0}", Player.MaxHealth);
                            base.OnPropertyChanged(Player, nameof(Player.MaxHealth));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.MaxHealth has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Sp:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Mana != e.ValueInt16)
                        {
                            Player.Mana = e.ValueInt16;
                            Logger.Info("Player.Mana = {0}", Player.Mana);
                            base.OnPropertyChanged(Player, nameof(Player.Mana));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Mana has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.MaxSp:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.MaxMana != e.ValueInt16)
                        {
                            Player.MaxMana = e.ValueInt16;
                            Logger.Info("Player.MaxMana = {0}", Player.MaxMana);
                            base.OnPropertyChanged(Player, nameof(Player.MaxMana));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.MaxMana has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Grace:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Grace != e.ValueInt16)
                        {
                            Player.Grace = e.ValueInt16;
                            Logger.Info("Player.Grace = {0}", Player.Grace);
                            base.OnPropertyChanged(Player, nameof(Player.Grace));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Grace has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.MaxGrace:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.MaxGrace != e.ValueInt16)
                        {
                            Player.MaxGrace = e.ValueInt16;
                            Logger.Info("Player.MaxGrace = {0}", Player.MaxGrace);
                            base.OnPropertyChanged(Player, nameof(Player.MaxGrace));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.MaxGrace has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Exp:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Experience != (UInt64)e.ValueInt16)
                        {
                            Player.Experience = (UInt64)e.ValueInt16;
                            Logger.Info("Player.Experience = {0}", Player.Experience);
                            base.OnPropertyChanged(Player, nameof(Player.Experience));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Experience has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Exp64:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.UInt64)
                    {
                        if (Player.Experience != e.ValueUInt64)
                        {
                            Player.Experience = e.ValueUInt64;
                            Logger.Info("Player.Experience = {0}", Player.Experience);
                            base.OnPropertyChanged(Player, nameof(Player.Experience));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Experience has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Level:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Level != e.ValueInt16)
                        {
                            Player.Level = e.ValueInt16;
                            Logger.Info("Player.Level = {0}", Player.Level);
                            base.OnPropertyChanged(Player, nameof(Player.Level));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Level has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Food:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Food != e.ValueInt16)
                        {
                            Player.Food = e.ValueInt16;
                            Logger.Info("Player.Food = {0}", Player.Food);
                            base.OnPropertyChanged(Player, nameof(Player.Food));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Food has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Title:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.String)
                    {
                        var tmpTitle = e.ValueString;

                        //Title prefix is currently "Player: ", but lets futureproof it
                        var splitString = tmpTitle.Split(new char[] { ':' }, 2);
                        if ((splitString != null) && (splitString.Length >= 2))
                        {
                            tmpTitle = splitString[1].Trim();
                        }

                        if (Player.Title != tmpTitle)
                        {
                            Player.Title = tmpTitle;
                            Logger.Info("Player.Title = {0}", Player.Title);
                            base.OnPropertyChanged(Player, nameof(Player.Title));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Title has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Range:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.String)
                    {
                        var rangeType = string.Empty;
                        var rangeValue = e.ValueString;

                        //Range types are currently "Range: ", "Skill: ", "Builder: "
                        //but lets futureproof it
                        var splitString = rangeValue.Split(new char[] { ':' }, 2);
                        if ((splitString != null) && (splitString.Length >= 2))
                        {
                            rangeType = splitString[0].Trim();
                            rangeValue = splitString[1].Trim();
                        }

                        if (Player.Range != rangeValue)
                        {
                            Player.Range = rangeValue;
                            Logger.Info("Player.Range = {0}", Player.Range);
                            base.OnPropertyChanged(Player, nameof(Player.Range));
                        }

                        if (Player.RangeType != rangeType)
                        {
                            Player.RangeType = rangeType;
                            Logger.Info("Player.RangeType = {0}", Player.RangeType);
                            base.OnPropertyChanged(Player, nameof(Player.RangeType));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Range has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.WeightLim:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Float)
                    {
                        if (Player.WeightLimit != e.ValueFloat)
                        {
                            Player.WeightLimit = e.ValueFloat;
                            Logger.Info("Player.WeightLimit = {0}", Player.WeightLimit);
                            base.OnPropertyChanged(Player, nameof(Player.WeightLimit));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.WeightLimit has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Flags:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.StatFlags != (NewClient.StatFlags)e.ValueInt16)
                        {
                            Player.StatFlags = (NewClient.StatFlags)e.ValueInt16;
                            Logger.Info("Player.StatFlags = {0}", Player.StatFlags);
                            base.OnPropertyChanged(Player, nameof(Player.StatFlags));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.StatFlags has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.CharacterFlags:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.UInt32)
                    {
                        if (Player.CharacterFlags != (NewClient.CharacterFlags)e.ValueUInt32)
                        {
                            Player.CharacterFlags = (NewClient.CharacterFlags)e.ValueUInt32;
                            Logger.Info("Player.CharacterFlags = {0}", Player.CharacterFlags);
                            base.OnPropertyChanged(Player, nameof(Player.CharacterFlags));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.CharacterFlags has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Str:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Strength != e.ValueInt16)
                        {
                            Player.Strength = e.ValueInt16;
                            Logger.Info("Player.Strength = {0}", Player.Strength);
                            base.OnPropertyChanged(Player, nameof(Player.Strength));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Strength has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Int:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Intellegence != e.ValueInt16)
                        {
                            Player.Intellegence = e.ValueInt16;
                            Logger.Info("Player.Intellegence = {0}", Player.Intellegence);
                            base.OnPropertyChanged(Player, nameof(Player.Intellegence));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Intellegence has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Pow:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Power != e.ValueInt16)
                        {
                            Player.Power = e.ValueInt16;
                            Logger.Info("Player.Power = {0}", Player.Power);
                            base.OnPropertyChanged(Player, nameof(Player.Power));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Power has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Wis:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Wisdom != e.ValueInt16)
                        {
                            Player.Wisdom = e.ValueInt16;
                            Logger.Info("Player.Wisdom = {0}", Player.Wisdom);
                            base.OnPropertyChanged(Player, nameof(Player.Wisdom));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Wisdom has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Dex:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Dexterity != e.ValueInt16)
                        {
                            Player.Dexterity = e.ValueInt16;
                            Logger.Info("Player.Dexterity = {0}", Player.Dexterity);
                            base.OnPropertyChanged(Player, nameof(Player.Dexterity));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Dexterity has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Con:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Constitution != e.ValueInt16)
                        {
                            Player.Constitution = e.ValueInt16;
                            Logger.Info("Player.Constitution = {0}", Player.Constitution);
                            base.OnPropertyChanged(Player, nameof(Player.Constitution));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Constitution has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Cha:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Charisma != e.ValueInt16)
                        {
                            Player.Charisma = e.ValueInt16;
                            Logger.Info("Player.Charisma = {0}", Player.Charisma);
                            base.OnPropertyChanged(Player, nameof(Player.Charisma));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Charisma has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.RaceStr:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.RaceStrength != e.ValueInt16)
                        {
                            Player.RaceStrength = e.ValueInt16;
                            Logger.Info("Player.RaceStrength = {0}", Player.RaceStrength);
                            base.OnPropertyChanged(Player, nameof(Player.RaceStrength));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.RaceStrength has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.RaceInt:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.RaceIntellegence != e.ValueInt16)
                        {
                            Player.RaceIntellegence = e.ValueInt16;
                            Logger.Info("Player.RaceIntellegence = {0}", Player.RaceIntellegence);
                            base.OnPropertyChanged(Player, nameof(Player.RaceIntellegence));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.RaceIntellegence has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.RacePow:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.RacePower != e.ValueInt16)
                        {
                            Player.RacePower = e.ValueInt16;
                            Logger.Info("Player.RacePower = {0}", Player.RacePower);
                            base.OnPropertyChanged(Player, nameof(Player.RacePower));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.RacePower has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.RaceWis:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.RaceWisdom != e.ValueInt16)
                        {
                            Player.RaceWisdom = e.ValueInt16;
                            Logger.Info("Player.RaceWisdom = {0}", Player.RaceWisdom);
                            base.OnPropertyChanged(Player, nameof(Player.RaceWisdom));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.RaceWisdom has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.RaceDex:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.RaceDexterity != e.ValueInt16)
                        {
                            Player.RaceDexterity = e.ValueInt16;
                            Logger.Info("Player.RaceDexterity = {0}", Player.RaceDexterity);
                            base.OnPropertyChanged(Player, nameof(Player.RaceDexterity));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.RaceDexterity has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.RaceCon:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.RaceConstitution != e.ValueInt16)
                        {
                            Player.RaceConstitution = e.ValueInt16;
                            Logger.Info("Player.RaceConstitution = {0}", Player.RaceConstitution);
                            base.OnPropertyChanged(Player, nameof(Player.RaceConstitution));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.RaceConstitution has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.RaceCha:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.RaceCharisma != e.ValueInt16)
                        {
                            Player.RaceCharisma = e.ValueInt16;
                            Logger.Info("Player.RaceCharisma = {0}", Player.RaceCharisma);
                            base.OnPropertyChanged(Player, nameof(Player.RaceCharisma));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.RaceCharisma has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.BaseStr:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.BaseStrength != e.ValueInt16)
                        {
                            Player.BaseStrength = e.ValueInt16;
                            Logger.Info("Player.BaseStrength = {0}", Player.BaseStrength);
                            base.OnPropertyChanged(Player, nameof(Player.BaseStrength));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.BaseStrength has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.BaseInt:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.BaseIntellegence != e.ValueInt16)
                        {
                            Player.BaseIntellegence = e.ValueInt16;
                            Logger.Info("Player.BaseIntellegence = {0}", Player.BaseIntellegence);
                            base.OnPropertyChanged(Player, nameof(Player.BaseIntellegence));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.BaseIntellegence has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.BasePow:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.BasePower != e.ValueInt16)
                        {
                            Player.BasePower = e.ValueInt16;
                            Logger.Info("Player.BasePower = {0}", Player.BasePower);
                            base.OnPropertyChanged(Player, nameof(Player.BasePower));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.BasePower has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.BaseWis:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.BaseWisdom != e.ValueInt16)
                        {
                            Player.BaseWisdom = e.ValueInt16;
                            Logger.Info("Player.BaseWisdom = {0}", Player.BaseWisdom);
                            base.OnPropertyChanged(Player, nameof(Player.BaseWisdom));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.BaseWisdom has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.BaseDex:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.BaseDexterity != e.ValueInt16)
                        {
                            Player.BaseDexterity = e.ValueInt16;
                            Logger.Info("Player.BaseDexterity = {0}", Player.BaseDexterity);
                            base.OnPropertyChanged(Player, nameof(Player.BaseDexterity));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.BaseDexterity has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.BaseCon:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.BaseConstitution != e.ValueInt16)
                        {
                            Player.BaseConstitution = e.ValueInt16;
                            Logger.Info("Player.BaseConstitution = {0}", Player.BaseConstitution);
                            base.OnPropertyChanged(Player, nameof(Player.BaseConstitution));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.BaseConstitution has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.BaseCha:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.BaseCharisma != e.ValueInt16)
                        {
                            Player.BaseCharisma = e.ValueInt16;
                            Logger.Info("Player.BaseCharisma = {0}", Player.BaseCharisma);
                            base.OnPropertyChanged(Player, nameof(Player.BaseCharisma));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.BaseCharisma has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.AppliedStr:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.AppliedStrength != e.ValueInt16)
                        {
                            Player.AppliedStrength = e.ValueInt16;
                            Logger.Info("Player.AppliedStrength = {0}", Player.AppliedStrength);
                            base.OnPropertyChanged(Player, nameof(Player.AppliedStrength));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.AppliedStrength has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.AppliedInt:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.AppliedIntellegence != e.ValueInt16)
                        {
                            Player.AppliedIntellegence = e.ValueInt16;
                            Logger.Info("Player.AppliedIntellegence = {0}", Player.AppliedIntellegence);
                            base.OnPropertyChanged(Player, nameof(Player.AppliedIntellegence));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.AppliedIntellegence has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.AppliedPow:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.AppliedPower != e.ValueInt16)
                        {
                            Player.AppliedPower = e.ValueInt16;
                            Logger.Info("Player.AppliedPower = {0}", Player.AppliedPower);
                            base.OnPropertyChanged(Player, nameof(Player.AppliedPower));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.AppliedPower has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.AppliedWis:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.AppliedWisdom != e.ValueInt16)
                        {
                            Player.AppliedWisdom = e.ValueInt16;
                            Logger.Info("Player.AppliedWisdom = {0}", Player.AppliedWisdom);
                            base.OnPropertyChanged(Player, nameof(Player.AppliedWisdom));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.AppliedWisdom has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.AppliedDex:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.AppliedDexterity != e.ValueInt16)
                        {
                            Player.AppliedDexterity = e.ValueInt16;
                            Logger.Info("Player.AppliedDexterity = {0}", Player.AppliedDexterity);
                            base.OnPropertyChanged(Player, nameof(Player.AppliedDexterity));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.AppliedDexterity has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.AppliedCon:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.AppliedConstitution != e.ValueInt16)
                        {
                            Player.AppliedConstitution = e.ValueInt16;
                            Logger.Info("Player.AppliedContitution = {0}", Player.AppliedConstitution);
                            base.OnPropertyChanged(Player, nameof(Player.AppliedConstitution));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.AppliedContitution has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.AppliedCha:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.AppliedCharisma != e.ValueInt16)
                        {
                            Player.AppliedCharisma = e.ValueInt16;
                            Logger.Info("Player.AppliedCharisma = {0}", Player.AppliedCharisma);
                            base.OnPropertyChanged(Player, nameof(Player.AppliedCharisma));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.AppliedCharisma has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Speed:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Float)
                    {
                        if (Player.Speed != e.ValueFloat)
                        {
                            Player.Speed = e.ValueFloat;
                            Logger.Info("Player.Speed = {0}", Player.Speed);
                            base.OnPropertyChanged(Player, nameof(Player.Speed));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Speed has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.GodName:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.String)
                    {
                        var tmpGodName = e.ValueString;
                        if (tmpGodName == "none")
                            tmpGodName = string.Empty;

                        if (Player.GodName != tmpGodName)
                        {
                            Player.GodName = tmpGodName;
                            Logger.Info("Player.GodName = {0}", Player.GodName);
                            base.OnPropertyChanged(Player, nameof(Player.GodName));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.GodName has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Overload:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Float)
                    {
                        if (Player.Overload != e.ValueFloat)
                        {
                            Player.Overload = e.ValueFloat;
                            Logger.Info("Player.Overload = {0}", Player.Overload);
                            base.OnPropertyChanged(Player, nameof(Player.Overload));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Overload has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.GolemHp:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.GolemHealth != e.ValueInt16)
                        {
                            Player.GolemHealth = e.ValueInt16;
                            Logger.Info("Player.GolemHealth = {0}", Player.GolemHealth);
                            base.OnPropertyChanged(Player, nameof(Player.GolemHealth));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.GolemHealth has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.GolemMaxHp:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.GolemMaxHealth != e.ValueInt16)
                        {
                            Player.GolemMaxHealth = e.ValueInt16;
                            Logger.Info("Player.GolemMaxHealth = {0}", Player.GolemMaxHealth);
                            base.OnPropertyChanged(Player, nameof(Player.GolemMaxHealth));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.GolemMaxHealth has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResPhys:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistPhysical != e.ValueInt16)
                        {
                            Player.ResistPhysical = e.ValueInt16;
                            Logger.Info("Player.ResistPhysical = {0}", Player.ResistPhysical);
                            base.OnPropertyChanged(Player, nameof(Player.ResistPhysical));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.ResistPhysical has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResMag:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistMagic != e.ValueInt16)
                        {
                            Player.ResistMagic = e.ValueInt16;
                            Logger.Info("Player.ResistMagic = {0}", Player.ResistMagic);
                            base.OnPropertyChanged(Player, nameof(Player.ResistMagic));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.ResistMagic has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResFire:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistFire != e.ValueInt16)
                        {
                            Player.ResistFire = e.ValueInt16;
                            Logger.Info("Player.ResistFire = {0}", Player.ResistFire);
                            base.OnPropertyChanged(Player, nameof(Player.ResistFire));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.ResistFire has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResElec:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistElectricity != e.ValueInt16)
                        {
                            Player.ResistElectricity = e.ValueInt16;
                            Logger.Info("Player.ResistElectricity = {0}", Player.ResistElectricity);
                            base.OnPropertyChanged(Player, nameof(Player.ResistElectricity));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.ResistElectricity has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResCold:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistCold != e.ValueInt16)
                        {
                            Player.ResistCold = e.ValueInt16;
                            Logger.Info("Player.ResistCold = {0}", Player.ResistCold);
                            base.OnPropertyChanged(Player, nameof(Player.ResistCold));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.ResistCold has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResConf:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistConfusion != e.ValueInt16)
                        {
                            Player.ResistConfusion = e.ValueInt16;
                            Logger.Info("Player.ResistConfusion = {0}", Player.ResistConfusion);
                            base.OnPropertyChanged(Player, nameof(Player.ResistConfusion));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.ResistConfusion has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResAcid:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistAcid != e.ValueInt16)
                        {
                            Player.ResistAcid = e.ValueInt16;
                            Logger.Info("Player.ResistAcid = {0}", Player.ResistAcid);
                            base.OnPropertyChanged(Player, nameof(Player.ResistAcid));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.ResistAcid has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResDrain:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistDrain != e.ValueInt16)
                        {
                            Player.ResistDrain = e.ValueInt16;
                            Logger.Info("Player.ResistDrain = {0}", Player.ResistDrain);
                            base.OnPropertyChanged(Player, nameof(Player.ResistDrain));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.ResistDrain has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResGhosthit:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistGhostHit != e.ValueInt16)
                        {
                            Player.ResistGhostHit = e.ValueInt16;
                            Logger.Info("Player.ResistGhostHit = {0}", Player.ResistGhostHit);
                            base.OnPropertyChanged(Player, nameof(Player.ResistGhostHit));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.ResistGhostHit has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResPoison:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistPoison != e.ValueInt16)
                        {
                            Player.ResistPoison = e.ValueInt16;
                            Logger.Info("Player.ResistPoison = {0}", Player.ResistPoison);
                            base.OnPropertyChanged(Player, nameof(Player.ResistPoison));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.ResistPoison has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResSlow:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistSlow != e.ValueInt16)
                        {
                            Player.ResistSlow = e.ValueInt16;
                            Logger.Info("Player.ResistSlow = {0}", Player.ResistSlow);
                            base.OnPropertyChanged(Player, nameof(Player.ResistSlow));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.ResistSlow has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResPara:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistParalysis != e.ValueInt16)
                        {
                            Player.ResistParalysis = e.ValueInt16;
                            Logger.Info("Player.ResistParalysis = {0}", Player.ResistParalysis);
                            base.OnPropertyChanged(Player, nameof(Player.ResistParalysis));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.ResistParalysis has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.TurnUndead:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.TurnUndead != e.ValueInt16)
                        {
                            Player.TurnUndead = e.ValueInt16;
                            Logger.Info("Player.TurnUndead = {0}", Player.TurnUndead);
                            base.OnPropertyChanged(Player, nameof(Player.TurnUndead));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.TurnUndead has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResFear:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistFear != e.ValueInt16)
                        {
                            Player.ResistFear = e.ValueInt16;
                            Logger.Info("Player.ResistFear = {0}", Player.ResistFear);
                            base.OnPropertyChanged(Player, nameof(Player.ResistFear));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.ResistFear has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResDeplete:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistDeplete != e.ValueInt16)
                        {
                            Player.ResistDeplete = e.ValueInt16;
                            Logger.Info("Player.ResistDeplete = {0}", Player.ResistDeplete);
                            base.OnPropertyChanged(Player, nameof(Player.ResistDeplete));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.ResistDeplete has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResDeath:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistDeath != e.ValueInt16)
                        {
                            Player.ResistDeath = e.ValueInt16;
                            Logger.Info("Player.ResistDeath = {0}", Player.ResistDeath);
                            base.OnPropertyChanged(Player, nameof(Player.ResistDeath));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.ResistDeath has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResHolyword:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistHolyWord != e.ValueInt16)
                        {
                            Player.ResistHolyWord = e.ValueInt16;
                            Logger.Info("Player.ResistHolyWord = {0}", Player.ResistHolyWord);
                            base.OnPropertyChanged(Player, nameof(Player.ResistHolyWord));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.ResistHolyWord has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ResBlind:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ResistBlindness != e.ValueInt16)
                        {
                            Player.ResistBlindness = e.ValueInt16;
                            Logger.Info("Player.ResistBlindness = {0}", Player.ResistBlindness);
                            base.OnPropertyChanged(Player, nameof(Player.ResistBlindness));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.ResistBlindness has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Wc:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.WeaponClass != e.ValueInt16)
                        {
                            Player.WeaponClass = e.ValueInt16;
                            Logger.Info("Player.WeaponClass = {0}", Player.WeaponClass);
                            base.OnPropertyChanged(Player, nameof(Player.WeaponClass));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.WeaponClass has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.WeapSp:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Float)
                    {
                        if (Player.WeaponSpeed != e.ValueFloat)
                        {
                            Player.WeaponSpeed = e.ValueFloat;
                            Logger.Info("Player.WeaponSpeed = {0}", Player.WeaponSpeed);
                            base.OnPropertyChanged(Player, nameof(Player.WeaponSpeed));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.WeaponSpeed has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Dam:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Damage != e.ValueInt16)
                        {
                            Player.Damage = e.ValueInt16;
                            Logger.Info("Player.Damage = {0}", Player.Damage);
                            base.OnPropertyChanged(Player, nameof(Player.Damage));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Damage has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Ac:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ArmourClass != e.ValueInt16)
                        {
                            Player.ArmourClass = e.ValueInt16;
                            Logger.Info("Player.ArmourClass = {0}", Player.ArmourClass);
                            base.OnPropertyChanged(Player, nameof(Player.ArmourClass));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.ArmourClass has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.Armour:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.Armour != e.ValueInt16)
                        {
                            Player.Armour = e.ValueInt16;
                            Logger.Info("Player.Armour = {0}", Player.Armour);
                            base.OnPropertyChanged(Player, nameof(Player.Armour));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.Armour has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.ItemPower:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.Int16)
                    {
                        if (Player.ItemPower != e.ValueInt16)
                        {
                            Player.ItemPower = e.ValueInt16;
                            Logger.Info("Player.ItemPower = {0}", Player.ItemPower);
                            base.OnPropertyChanged(Player, nameof(Player.ItemPower));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.ItemPower has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.SpellAttune:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.UInt32)
                    {
                        if (Player.SpellAttune != e.ValueUInt32)
                        {
                            Player.SpellAttune = e.ValueUInt32;
                            Logger.Info("Player.SpellAttune = {0}", Player.SpellAttune);
                            base.OnPropertyChanged(Player, nameof(Player.SpellAttune));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.SpellAttune has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.SpellRepel:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.UInt32)
                    {
                        if (Player.SpellRepel != e.ValueUInt32)
                        {
                            Player.SpellRepel = e.ValueUInt32;
                            Logger.Info("Player.SpellRepel = {0}", Player.SpellRepel);
                            base.OnPropertyChanged(Player, nameof(Player.SpellRepel));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.SpellRepel has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.CharacterStats.SpellDeny:
                    if (e.DataType == MessageHandler.StatEventArgs.StatDataTypes.UInt32)
                    {
                        if (Player.SpellDeny != e.ValueUInt32)
                        {
                            Player.SpellDeny = e.ValueUInt32;
                            Logger.Info("Player.SpellDeny = {0}", Player.SpellDeny);
                            base.OnPropertyChanged(Player, nameof(Player.SpellDeny));
                        }
                    }
                    else
                    {
                        Logger.Warning("Player.SpellDeny has wrong datatype: {0}", e.DataType);
                    }
                    break;

                //the leftover stats are actually skills
                default:
                    Logger.Warning("Player has unnapped stat: {0}", e.Stat);

                    if (!Player.UnmappedStats.ContainsKey(e.Stat) || (Player.UnmappedStats[e.Stat] != e.ValueAsString))
                    {
                        Player.UnmappedStats[e.Stat] = e.ValueAsString;
                        Logger.Info("Player.UnmappedStats:{0} = {1}", e.Stat, Player.UnmappedStats[e.Stat]);
                        base.OnPropertyChanged(Player, nameof(Player.UnmappedStats) + "|" + e.Stat.ToString());
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

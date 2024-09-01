using CrossfireRPG.ServerInterface.Definitions;
using System;
using System.Collections.Generic;
using TheBlackRoom.System.Extensions;

namespace CrossfireRPG.ServerInterface.Managers.PlayerManagement
{
    public class Player : DataObject
    {
        /// <summary>
        /// Tag of 0 indicates no active player
        /// </summary>
        public UInt32 PlayerTag { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public UInt32 Face { get; set; } = 0;

        public float Weight { get; set; } = 0;
        public float WeightLimit { get; set; } = 0;

        public UInt32 Health { get; set; } = 0;
        public Int16 MaxHealth { get; set; } = 0;
        public Int16 Mana { get; set; } = 0;
        public Int16 MaxMana { get; set; } = 0;
        public Int16 Grace { get; set; } = 0;
        public Int16 MaxGrace { get; set; } = 0;
        public Int16 Food { get; set; } = 0;
        public UInt64 Experience { get; set; } = 0;
        public Int16 Level { get; set; } = 0;
        public string Range { get; set; } = string.Empty;
        public string RangeType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public NewClient.StatFlags StatFlags { get; set; } = 0;
        public NewClient.CharacterFlags CharacterFlags { get; set; } = 0;

        /// <summary>
        /// Character Current Strength
        /// </summary>
        public Int16 Strength { get; set; } = 0;
        public Int16 Intellegence { get; set; } = 0;
        public Int16 Power { get; set; } = 0;
        public Int16 Wisdom { get; set; } = 0;
        public Int16 Dexterity { get; set; } = 0;
        public Int16 Constitution { get; set; } = 0;
        public Int16 Charisma { get; set; } = 0;

        /// <summary>
        /// Character Maximum Strength
        /// </summary>
        public Int16 RaceStrength { get; set; } = 0;
        public Int16 RaceIntellegence { get; set; } = 0;
        public Int16 RacePower { get; set; } = 0;
        public Int16 RaceWisdom { get; set; } = 0;
        public Int16 RaceDexterity { get; set; } = 0;
        public Int16 RaceConstitution { get; set; } = 0;
        public Int16 RaceCharisma { get; set; } = 0;

        /// <summary>
        /// Character Base Strength
        /// </summary>
        public Int16 BaseStrength { get; set; } = 0;
        public Int16 BaseIntellegence { get; set; } = 0;
        public Int16 BasePower { get; set; } = 0;
        public Int16 BaseWisdom { get; set; } = 0;
        public Int16 BaseDexterity { get; set; } = 0;
        public Int16 BaseConstitution { get; set; } = 0;
        public Int16 BaseCharisma { get; set; } = 0;

        /// <summary>
        /// Character Strength Item Bonus
        /// </summary>
        public Int16 AppliedStrength { get; set; } = 0;
        public Int16 AppliedIntellegence { get; set; } = 0;
        public Int16 AppliedPower { get; set; } = 0;
        public Int16 AppliedWisdom { get; set; } = 0;
        public Int16 AppliedDexterity { get; set; } = 0;
        public Int16 AppliedConstitution { get; set; } = 0;
        public Int16 AppliedCharisma { get; set; } = 0;

        public float Speed { get; set; } = 0;
        public string GodName { get; set; } = string.Empty;
        public float Overload { get; set; } = 0;
        public Int16 GolemHealth { get; set; } = 0;
        public Int16 GolemMaxHealth { get; set; } = 0;

        public Int16 ResistPhysical { get; set; } = 0;
        public Int16 ResistMagic { get; set; } = 0;
        public Int16 ResistFire { get; set; } = 0;
        public Int16 ResistElectricity { get; set; } = 0;
        public Int16 ResistCold { get; set; } = 0;
        public Int16 ResistConfusion { get; set; } = 0;
        public Int16 ResistAcid { get; set; } = 0;
        public Int16 ResistDrain { get; set; } = 0;
        public Int16 ResistGhostHit { get; set; } = 0;
        public Int16 ResistPoison { get; set; } = 0;
        public Int16 ResistSlow { get; set; } = 0;
        public Int16 ResistParalysis { get; set; } = 0;
        public Int16 TurnUndead { get; set; } = 0;
        public Int16 ResistFear { get; set; } = 0;
        public Int16 ResistDeplete { get; set; } = 0;
        public Int16 ResistDeath { get; set; } = 0;
        public Int16 ResistHolyWord { get; set; } = 0;
        public Int16 ResistBlindness { get; set; } = 0;
        public Int16 WeaponClass { get; set; } = 0;
        public float WeaponSpeed { get; set; } = 0;
        public Int16 Damage { get; set; } = 0;
        public Int16 ArmourClass { get; set; } = 0;
        public Int16 Armour { get; set; } = 0;
        public Int16 ItemPower { get; set; } = 0;

        /// <summary>
        /// Attuned SpellPaths (See Info.SpellPath for Paths)
        /// </summary>
        public UInt32 SpellAttune { get; set; } = 0;

        /// <summary>
        /// Repelled SpellPaths (See Info.SpellPath Paths)
        /// </summary>
        public UInt32 SpellRepel { get; set; } = 0;

        /// <summary>
        /// Denied SpellPaths (See Info.SpellPath Paths)
        /// </summary>
        public UInt32 SpellDeny { get; set; } = 0;

        public Dictionary<NewClient.CharacterStats, string> UnmappedStats { get; } =
            new Dictionary<NewClient.CharacterStats, string>();

        public bool ValidPlayer => PlayerTag != 0;

        public string GetStat(NewClient.CharacterStats Stat)
        {
            switch (Stat)
            {
                case NewClient.CharacterStats.Str:
                    return this.Strength.ToString();

                case NewClient.CharacterStats.Int:
                    return this.Intellegence.ToString();

                case NewClient.CharacterStats.Pow:
                    return this.Power.ToString();

                case NewClient.CharacterStats.Wis:
                    return this.Wisdom.ToString();

                case NewClient.CharacterStats.Dex:
                    return this.Dexterity.ToString();

                case NewClient.CharacterStats.Con:
                    return this.Constitution.ToString();

                case NewClient.CharacterStats.Cha:
                    return this.Charisma.ToString();

                case NewClient.CharacterStats.RaceStr:
                    return this.RaceStrength.ToString();

                case NewClient.CharacterStats.RaceInt:
                    return this.RaceIntellegence.ToString();

                case NewClient.CharacterStats.RacePow:
                    return this.RacePower.ToString();

                case NewClient.CharacterStats.RaceWis:
                    return this.RaceWisdom.ToString();

                case NewClient.CharacterStats.RaceDex:
                    return this.RaceDexterity.ToString();

                case NewClient.CharacterStats.RaceCon:
                    return this.RaceConstitution.ToString();

                case NewClient.CharacterStats.RaceCha:
                    return this.RaceCharisma.ToString();

                case NewClient.CharacterStats.BaseStr:
                    return this.BaseStrength.ToString();

                case NewClient.CharacterStats.BaseInt:
                    return this.BaseIntellegence.ToString();

                case NewClient.CharacterStats.BasePow:
                    return this.BasePower.ToString();

                case NewClient.CharacterStats.BaseWis:
                    return this.BaseWisdom.ToString();

                case NewClient.CharacterStats.BaseDex:
                    return this.BaseDexterity.ToString();

                case NewClient.CharacterStats.BaseCon:
                    return this.BaseConstitution.ToString();

                case NewClient.CharacterStats.BaseCha:
                    return this.BaseCharisma.ToString();

                case NewClient.CharacterStats.AppliedStr:
                    return this.AppliedStrength.ToString();

                case NewClient.CharacterStats.AppliedInt:
                    return this.AppliedIntellegence.ToString();

                case NewClient.CharacterStats.AppliedPow:
                    return this.AppliedPower.ToString();

                case NewClient.CharacterStats.AppliedWis:
                    return this.AppliedWisdom.ToString();

                case NewClient.CharacterStats.AppliedDex:
                    return this.AppliedDexterity.ToString();

                case NewClient.CharacterStats.AppliedCon:
                    return this.AppliedConstitution.ToString();

                case NewClient.CharacterStats.AppliedCha:
                    return this.AppliedCharisma.ToString();

                case NewClient.CharacterStats.Hp:
                    return this.Health.ToString();

                case NewClient.CharacterStats.MaxHp:
                    return this.MaxHealth.ToString();

                case NewClient.CharacterStats.Sp:
                    return this.Mana.ToString();

                case NewClient.CharacterStats.MaxSp:
                    return this.MaxMana.ToString();

                case NewClient.CharacterStats.Grace:
                    return this.Grace.ToString();

                case NewClient.CharacterStats.MaxGrace:
                    return this.MaxGrace.ToString();

                case NewClient.CharacterStats.Food:
                    return this.Food.ToString();

                case NewClient.CharacterStats.Exp:
                case NewClient.CharacterStats.Exp64:
                    return this.Experience.ToString();

                case NewClient.CharacterStats.Level:
                    return this.Level.ToString();

                case NewClient.CharacterStats.Title:
                    return this.Title;

                case NewClient.CharacterStats.Speed:
                    return this.Speed.ToString("n2");

                case NewClient.CharacterStats.WeightLim:
                    return this.WeightLimit.ToString("n2");

                case NewClient.CharacterStats.Flags:
                    return this.StatFlags.GetDescriptions(", ");

                case NewClient.CharacterStats.CharacterFlags:
                    return this.CharacterFlags.GetDescriptions(", ");

                case NewClient.CharacterStats.GodName:
                    return this.GodName;

                case NewClient.CharacterStats.Overload:
                    return this.Overload.ToString("n2");

                case NewClient.CharacterStats.GolemHp:
                    return this.GolemHealth.ToString();

                case NewClient.CharacterStats.GolemMaxHp:
                    return this.GolemMaxHealth.ToString();

                case NewClient.CharacterStats.ResPhys:
                    return this.ResistPhysical.ToString();

                case NewClient.CharacterStats.ResMag:
                    return this.ResistMagic.ToString();

                case NewClient.CharacterStats.ResFire:
                    return this.ResistFire.ToString();

                case NewClient.CharacterStats.ResElec:
                    return this.ResistElectricity.ToString();

                case NewClient.CharacterStats.ResCold:
                    return this.ResistCold.ToString();

                case NewClient.CharacterStats.ResConf:
                    return this.ResistConfusion.ToString();

                case NewClient.CharacterStats.ResAcid:
                    return this.ResistAcid.ToString();

                case NewClient.CharacterStats.ResDrain:
                    return this.ResistDrain.ToString();

                case NewClient.CharacterStats.ResGhosthit:
                    return this.ResistGhostHit.ToString();

                case NewClient.CharacterStats.ResPoison:
                    return this.ResistPoison.ToString();

                case NewClient.CharacterStats.ResSlow:
                    return this.ResistSlow.ToString();

                case NewClient.CharacterStats.ResPara:
                    return this.ResistParalysis.ToString();

                case NewClient.CharacterStats.TurnUndead:
                    return this.TurnUndead.ToString();

                case NewClient.CharacterStats.ResFear:
                    return this.ResistFear.ToString();

                case NewClient.CharacterStats.ResDeplete:
                    return this.ResistDeplete.ToString();

                case NewClient.CharacterStats.ResDeath:
                    return this.ResistDeath.ToString();

                case NewClient.CharacterStats.ResHolyword:
                    return this.ResistHolyWord.ToString();

                case NewClient.CharacterStats.ResBlind:
                    return this.ResistBlindness.ToString();

                case NewClient.CharacterStats.Range:
                    return this.Range;

                case NewClient.CharacterStats.Wc:
                    return this.WeaponClass.ToString();

                case NewClient.CharacterStats.WeapSp:
                    return this.WeaponSpeed.ToString("n2");

                case NewClient.CharacterStats.Dam:
                    return this.Damage.ToString();

                case NewClient.CharacterStats.Ac:
                    return this.ArmourClass.ToString();

                case NewClient.CharacterStats.Armour:
                    return this.Armour.ToString();

                case NewClient.CharacterStats.ItemPower:
                    return this.ItemPower.ToString();

                case NewClient.CharacterStats.SpellAttune:
                    return this.SpellAttune.ToString();

                case NewClient.CharacterStats.SpellRepel:
                    return this.SpellRepel.ToString();

                case NewClient.CharacterStats.SpellDeny:
                    return this.SpellDeny.ToString();

                default:
                    if (UnmappedStats.TryGetValue(Stat, out var value))
                        return value;

                    return null;
            }
        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Name) ? PlayerTag.ToString() : Name;
        }
    }
}

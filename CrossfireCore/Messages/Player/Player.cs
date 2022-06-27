using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.Managers
{
    public class Player
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
        public string Title { get; set; } = string.Empty;
        public NewClient.StatFlags StatFlags { get; set; } = 0;
        public NewClient.CharacterFlags CharacterFlags { get; set; } = 0;

        /// <summary>
        /// Character Total Strength
        /// </summary>
        public Int16 Strength { get; set; } = 0;
        public Int16 Intellegence { get; set; } = 0;
        public Int16 Power { get; set; } = 0;
        public Int16 Wisdom { get; set; } = 0;
        public Int16 Dexterity { get; set; } = 0;
        public Int16 Constitution { get; set; } = 0;
        public Int16 Charisma { get; set; } = 0;

        /// <summary>
        /// Character Strength Racial Bonus
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
        public Int16 AppliedContitution { get; set; } = 0;
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

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Name) ? PlayerTag.ToString() : Name;
        }
    }
}

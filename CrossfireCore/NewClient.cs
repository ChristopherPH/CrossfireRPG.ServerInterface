using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossfireCore
{
    public static partial class NewClient
    {
        public enum NewDrawInfo //NDI
        {
            Black = 0,
            White = 1,
            Navy = 2,
            Red = 3,
            Orange = 4,
            Blue = 5,			// Dodger Blue
            DarkOrange = 6,		// DarkOrange2
            Green = 7,			// SeaGreen
            LightGreen = 8,		// DarkSeaGreen
            Grey = 9,
            Brown = 10,			//Sienna
            Gold = 11,
            Tan = 12,			//Khaki

            Unique = 0x100,		//Print immediately, don't buffer.
            All = 0x200,		//Inform all players of this message
            AllDMs = 0x400,		//Inform all logged in DMs. Used in case of errors. Overrides NDI_ALL
            NoTranslate = 0x800,
            Delayed = 0x1000,   //If set, then message is sent only after the player's tick completes.
                            //This allows sending eg quest information after some dialogue even
                            //though quest is processed before.
        }

        public const int NewDrawInfoColorMask = 0xff;

        //Magic Map constants
        public const int FaceFloor = 0x80;
        public const int FaceWall = 0x40;
        public const int FaceColourMask = 0xf;

        public enum CharacterStats
        {
            [Description("Health")]			Hp = 1,
            [Description("Max Health")]		MaxHp = 2,
            [Description("Mana")]			Sp = 3,
            [Description("Max Mana")]		MaxSp = 4,

            [Description("Strength")]		Str = 5,
            [Description("Intelligence")]	Int = 6,
            [Description("Wisdom")]			Wis = 7,
            [Description("Dexterity")]		Dex = 8,
            [Description("Constitution")]	Con = 9,
            [Description("Charisma")]		Cha = 10,

            [Description("Experience")]		Exp = 11,  /* No Longer Used */
            [Description("Level")]			Level = 12,
            [Description("Weapon Class")]	Wc = 13,
            [Description("Armour Class")]	Ac = 14,
            [Description("Damage")]			Dam = 15,
            [Description("Armour")]			Armour = 16,
            [Description("Speed")]			Speed = 17,
            [Description("Hunger")]			Food = 18,
            [Description("Weapon Speed")]	WeapSp = 19,
            [Description("Range")]			Range = 20,
            [Description("Title")]			Title = 21,
            [Description("Power")]			Pow = 22,
            [Description("Grace")]			Grace = 23,
            [Description("Max Grace")]		MaxGrace = 24,
			
            Flags = 25,

            [Description("Weight Limit")]	WeightLim = 26,
            [Description("Experience")]		Exp64 = 28,

            [Description("Spell Attune")]	SpellAttune = 29,
            [Description("Spell Repel")]	SpellRepel = 30,
            [Description("Spell Deny")]		SpellDeny = 31,

            [Description("Strength")]      RaceStr = 32,
            [Description("Intelligence")]  RaceInt = 33,
            [Description("Wisdom")]        RaceWis = 34,
            [Description("Dexterity")]     RaceDex = 35,
            [Description("Constitution")]  RaceCon = 36,
            [Description("Charisma")]      RaceCha = 37,
            [Description("Power")]         RacePow = 38,

            [Description("Strength")]      BaseStr = 39,
            [Description("Intelligence")]  BaseInt = 40,
            [Description("Wisdom")]        BaseWis = 41,
            [Description("Dexterity")]     BaseDex = 42,
            [Description("Constitution")]  BaseCon = 43,
            [Description("Charisma")]      BaseCha = 44,
            [Description("Power")]         BasePow = 45,

            [Description("Strength")]       AppliedStr = 46,		/* Str Changes From Gear Or Skills. */
            [Description("Intelligence")]   AppliedInt = 47,		/* Int Changes From Gear Or Skills. */
            [Description("Wisdom")]         AppliedWis = 48,		/* Wis Changes From Gear Or Skills. */
            [Description("Dexterity")]      AppliedDex = 49,		/* Dex Changes From Gear Or Skills. */
            [Description("Constitution")]   AppliedCon = 50,		/* Con Changes From Gear Or Skills. */
            [Description("Charisma")]       AppliedCha = 51,		/* Cha Changes From Gear Or Skills. */
            [Description("Power")]          AppliedPow = 52,        /* Pow Changes From Gear Or Skills. */

            [Description("Golem Health")]	    GolemHp = 53,           /* Golem's Current Hp = 0, If No Golem. */
            [Description("Golem Max HP")]	    GolemMaxHp = 54,		/* Golem's Max Hp = 0, If No Golem. */

            [Description("Character Flags")]    CharacterFlags = 55,
            [Description("God Name")]           GodName = 56,
            [Description("Overload")]           Overload = 57,
            [Description("Item Power")]         ItemPower = 58,

            [Description("Resist Physical")]	ResPhys = 100,
            [Description("Resist Magic")]		ResMag = 101,
            [Description("Resist Fire")]		ResFire = 102,
            [Description("Resist Electricity")]	ResElec = 103,
            [Description("Resist Cold")]		ResCold = 104,
            [Description("Resist Confusion")]	ResConf = 105,
            [Description("Resist Acid")]		ResAcid = 106,
            [Description("Resist Drain")]		ResDrain = 107,
            [Description("Resist Ghost Hit")]	ResGhosthit = 108,
            [Description("Resist Poison")]		ResPoison = 109,
            [Description("Resist Slow")]		ResSlow = 110,
            [Description("Resist Paralysis")]	ResPara = 111,
            [Description("Turn Undead")]		TurnUndead = 112,
            [Description("Resist Fear")]		ResFear = 113,
            [Description("Resist Deplete")]		ResDeplete = 114,
            [Description("Resist Death")]		ResDeath = 115,
            [Description("Resist Holy Word")]	ResHolyword = 116,
            [Description("Resist Blindness")]	ResBlind = 117,
        }

        public const int CharacterStats_ResistStart = 100;  /* Start Of Resistances (Inclusive) */
        public const int CharacterStats_ResistEnd = 117;    /* End Of Resistances (Inclusive)   */
        public const int CharacterStats_SkillInfo = 140;
        public const int CharacterStats_NumSkills = 50;

        public enum AccountCharacterLoginTypes
        {
            Name = 1,
            Class = 2,
            Race = 3,
            Level = 4,
            Face = 5,
            Party = 6,
            Map = 7,
            FaceNum = 8,
        }

        [Flags]
        public enum UpdateTypes : uint
        {
            Location = 0x01,
            Flags = 0x02,
            Weight = 0x04,
            Face = 0x08,
            Name = 0x10,
            Animation = 0x20,
            AnimationSpeed = 0x40,
            NumberOf = 0x80,
            All = 0xFF
        }


        [Flags]
        public enum ItemFlags : UInt32
        {
            None = 0,

            /// <summary>
            /// Bows, Wands, Rods
            /// </summary>
            Readied = 1,

            /// <summary>
            /// Weapons
            /// </summary>
            Wielded = 2,    //weapon

            /// <summary>
            /// Skills, Armour, Helmets, Shields, Rings, Boots, Gloves, Amulets, Girdles, Bracers, Cloaks
            /// </summary>
            Worn = 3,

            /// <summary>
            /// Containers
            /// </summary>
            Active = 4,

            /// <summary>
            /// Misc applied items
            /// </summary>
            Applied = 5,

            /// <summary>
            /// Bitmask to retrieve type of applied
            /// </summary>
            Applied_Mask = 0x000F,

            Unidentified = 0x0010,
            Unpaid = 0x0200,
            Magic = 0x0400,
            Cursed = 0x0800,
            Damned = 0x1000,
            Open = 0x2000,
            NoPick = 0x4000,
            Locked = 0x8000,
            Blessed = 0x0100,
            Read = 0x0020,		//this is past tense, as in the item has been read
        }

        [Flags]
        public enum UpdateSpellTypes : uint
        {
            Mana = 0x01,
            Grace = 0x02,
            Damage = 0x04,
        }

        [Flags]
        public enum StatFlags : UInt16
        {
            Firing = 0x01,
            Running = 0x02,
        }

        [Flags]
        public enum CharacterFlags : uint
        {
            /// <summary>
            /// Confused by a spell or an item
            /// </summary>
            Confused = 0x01,

            /// <summary>
            /// Poisoned
            /// </summary>
            Poison = 0x02,

            /// <summary>
            /// Blind
            /// </summary>
            Blind = 0x04,

            /// <summary>
            /// Has X-ray
            /// </summary>
            [Description("X-Ray")]
            XRay = 0x80,

            /// <summary>
            /// Has at least one disease
            /// </summary>
            Diseased = 0x10,

            /// <summary>
            /// Can drink some improvement potions
            /// </summary>
            [Description("Not Perfect")]
            NotPerfect = 0x20,

            /// <summary>
            /// 'hostile' flag is set
            /// </summary>
            Hostile = 0x40,

            /// <summary>
            /// Player is stealthy
            /// </summary>
            Stealthy = 0x80,

            /// <summary>
            /// Player is paralyzed
            /// </summary>
            Paralyzed = 0x100,

            /// <summary>
            /// Player is DM
            /// </summary>
            Wizard = 0x0200
        }
    }
}

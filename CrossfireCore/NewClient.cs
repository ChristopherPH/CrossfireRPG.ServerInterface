using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossfireCore
{
    public static class NewClient
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

        public enum MsgTypes
        {
            MOTD = 7,
            Admin = 8,
            Client = 20,
        }

		public const int NewDrawInfoColorMask = 0x00ff;
		public const int NewDrawInfoFlagMask = 0xff00;

        public enum MsgTypeAdmin
        {
            Rules = 1,
            News = 2,
            Player = 3,     /* Player coming/going/death */
            DM = 4,         /* DM related admin actions */
            HiScore = 5,    /* Hiscore list */
            LoadSave = 6,   /* load/save operations */
            Login = 7,      /* login messages/errors */
            Version = 8,    /* version info */
            Error = 9       /* Error on command, setup, etc */
        }

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

			RaceStr = 32,
			RaceInt = 33,
			RaceWis = 34,
			RaceDex = 35,
			RaceCon = 36,
			RaceCha = 37,
			RacePow = 38,

			BaseStr = 39,
			BaseInt = 40,
			BaseWis = 41,
			BaseDex = 42,
			BaseCon = 43,
			BaseCha = 44,
			BasePow = 45,

			AppliedStr = 46,		/* Str Changes From Gear Or Skills. */
			AppliedInt = 47,		/* Int Changes From Gear Or Skills. */
			AppliedWis = 48,		/* Wis Changes From Gear Or Skills. */
			AppliedDex = 49,		/* Dex Changes From Gear Or Skills. */
			AppliedCon = 50,		/* Con Changes From Gear Or Skills. */
			AppliedCha = 51,		/* Cha Changes From Gear Or Skills. */
			AppliedPow = 52,		/* Pow Changes From Gear Or Skills. */

			GolemHp = 53,			/* Golem's Current Hp = 0, If No Golem. */
			GolemMaxHp = 54,		/* Golem's Max Hp = 0, If No Golem. */

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
		public enum ItemFlags : uint
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
	}
}

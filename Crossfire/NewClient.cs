using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire
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
			Hp = 1,
			MaxHp = 2,
			Sp = 3,
			MaxSp = 4,
			Str = 5,
			Int = 6,
			Wis = 7,
			Dex = 8,
			Con = 9,
			Cha = 10,
			Exp = 11,  /* No Longer Used */
			Level = 12,
			Wc = 13,
			Ac = 14,
			Dam = 15,
			Armour = 16,
			Speed = 17,
			Food = 18,
			WeapSp = 19,
			Range = 20,
			Title = 21,
			Pow = 22,
			Grace = 23,
			MaxGrace = 24,
			Flags = 25,
			WeightLim = 26,
			Exp64 = 28,

			SpellAttune = 29,
			SpellRepel = 30,
			SpellDeny = 31,

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

			ResPhys = 100,
			ResMag = 101,
			ResFire = 102,
			ResElec = 103,
			ResCold = 104,
			ResConf = 105,
			ResAcid = 106,
			ResDrain = 107,
			ResGhosthit = 108,
			ResPoison = 109,
			ResSlow = 110,
			ResPara = 111,
			TurnUndead = 112,
			ResFear = 113,
			ResDeplete = 114,
			ResDeath = 115,
			ResHolyword = 116,
			ResBlind = 117,
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
	}
}

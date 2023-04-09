using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore
{
    public static class ClientTypes
    {
        public static string GetClientTypeInfo(UInt16 ClientType, out string Group)
        {
			Group = "Unknown";

			if (ClientType <= 0)
				return Group;

			//Specials - items that should be very noticable to the player.
			if (ClientType >= 1 && ClientType <= 49)
			{
				Group = "Special";

				switch (ClientType)
                {
					case 1: return "Bomb"; //47
					case 41: return "Power Crystal"; //156

					default: return Group;
                }
			}

			//Containers - put near top to make things easier for the player.
			if (ClientType >= 50 && ClientType <= 99)
			{
				Group = "Containers";

				switch (ClientType)
				{
					case 51: return "Big Container"; //122 - chests, sacks, etc
					case 55: return "Small Container"; //pouches
					case 60: return "Specialized Container"; //quivers, key rings

					default: return Group;
				}
			}

			//hand held weapons
			if (ClientType >= 100 && ClientType <= 149)
			{
				Group = "Hand Held Weapons";

				switch (ClientType)
				{
					case 100: return "Artifact Weapon"; //15
					case 101: return "Edge Weapon"; //sword, scimitar, etc
					case 106: return "Axe";
					case 121: return "Club";
					case 126: return "Hammer";
					case 129: return "Mace";
					case 136: return "Pole Arm";
					case 141: return "Chained Weapon";
					case 145: return "Oddball Weapon"; //magnify glass, stake, taifu, shovel, etc

					default: return Group;
				}
			}

			//ranged weapons
			if (ClientType >= 150 && ClientType <= 199)
			{
				Group = "Ranged Weapons & Ammo";

				switch (ClientType)
				{
					case 150: return "Artifact Bow"; //14
					case 151: return "Bow"; //14
					case 159: return "Arrow"; //13
					case 161: return "Crossbow";
					case 165: return "Bolt";

					default: return Group;
				}
			}

			//Armor, shields, helms, etc.Give each subtype a group
			if (ClientType >= 250 && ClientType <= 399)
			{
				Group = "Wearables";

				if (ClientType >= 250 && ClientType <= 259)
				{
					Group = "Bodywear"; //(mails - 16) - ordered roughly in order of value

					switch (ClientType)
					{
						case 250: return "Artifact";
						case 251: return "Dragonmail";
						case 252: return "Plate Mail";
						case 253: return "Chain/Scale/Ring Mail";
						case 254: return "Leather Armor";
						case 255: return "Dress";
						case 256: return "Robe/Tunic";
						case 257: return "Apron";

						default: return Group;
					}
				}

				if (ClientType >= 260 && ClientType <= 269)
				{
					Group = "Shields";

					switch (ClientType)
					{
						case 260: return "Artifact Shield"; //33
						case 261: return "Shield"; //33

						default: return Group;
					}
				}

				if (ClientType >= 270 && ClientType <= 279)
				{
					Group = "Headwear";

					switch (ClientType)
					{
						case 270: return "Artifact Helmet"; //34
						case 271: return "Helmet"; //34
						case 272: return "Turban"; //34
						case 273: return "Wig"; //34
						case 275: return "Eyeglasses"; //34

						default: return Group;
					}
				}

				if (ClientType >= 280 && ClientType <= 289)
				{
					Group = "Cloaks";

					switch (ClientType)
					{
						case 280: return "Artifact Cloak"; //87
						case 281: return "Cloak"; //87

						default: return Group;
					}
				}

				if (ClientType >= 290 && ClientType <= 299)
				{
					Group = "Boots";

					switch (ClientType)
					{
						case 290: return "Artifact Boots"; //99
						case 291: return "Boots"; //99

						default: return Group;
					}
				}

				if (ClientType >= 300 && ClientType <= 309)
				{
					Group = "Gloves";

					switch (ClientType)
					{
						case 300: return "Artifact Gloves"; //100
						case 301: return "Gloves"; //100
						case 305: return "Gauntlets"; //100

						default: return Group;
					}
				}

				if (ClientType >= 310 && ClientType <= 319)
				{
					Group = "Bracers";

					switch (ClientType)
					{
						case 300: return "Artifact Bracers"; //104
						case 301: return "Bracers"; //104

						default: return Group;
					}
				}

				if (ClientType >= 320 && ClientType <= 329)
				{
					Group = "Girdles";

					switch (ClientType)
					{
						case 321: return "Girdle"; //113

						default: return Group;
					}
				}

				if (ClientType >= 380 && ClientType <= 389)
				{
					Group = "Amulets";

					switch (ClientType)
					{
						case 381: return "Amulet"; //39

						default: return Group;
					}
				}

				if (ClientType >= 390 && ClientType <= 399)
				{
					Group = "Rings";

					switch (ClientType)
					{
						case 390: return "Artifact Ring"; //70
						case 391: return "Ring"; //70

						default: return Group;
					}
				}
			}

			//Skill objects -these are items that give you a skill, eg lockpicks, talismens, etc.
			if (ClientType >= 450 && ClientType <= 499)
			{
				Group = "Skill Objects";

				switch (ClientType)
				{
					case 451: return "Skill Object"; //43
					case 461: return "Trap Parts"; //76

					default: return Group;
				}
			}

			if (ClientType >= 600 && ClientType <= 649)
			{
				Group = "Food & Alchemy";

				switch (ClientType)
				{
					case 601: return "Food"; //6
					//case 611: return "Poison"; //7
					case 611: return "Drink"; //54
					case 620: return "Flesh"; //72
					case 622: return "Corpse"; //157
					case 624: return "Quasi Food"; 
					case 625: return "Head, Eyes, Tongue, Teeth"; //72
					case 626: return "Appendages"; //legs, arms, hands, feet, fingers
					case 627: return "Misc Parts"; //ichors, scales, hearts, livers, skin 72
					case 628: return "Dust";
					case 641: return "Raw"; //73
					case 642: return "Refined"; //true lead, mercury, etc
					case 649: return "Poison"; //vial_poison.arc

					default: return Group;
				}
			}

			if (ClientType >= 650 && ClientType <= 699)
			{
				Group = "Single Use Spell Items";

				switch (ClientType)
				{
					case 651: return "Potion"; //5
					case 652: return "Balm/Dust"; //5
					case 653: return "Figurine"; //5
					case 661: return "Scroll"; //111

					default: return Group;
				}
			}

			if (ClientType >= 700 && ClientType <= 749)
			{
				Group = "Ranged Spell Items";

				switch (ClientType)
				{
					case 701: return "Heavy Rod"; //3
					case 702: return "Light Rod"; //3
					case 711: return "Wand"; //109
					case 712: return "Staff";
					case 721: return "Horn"; //35

					default: return Group;
				}
			}

			if (ClientType >= 800 && ClientType <= 849)
			{
				Group = "Keys";

				switch (ClientType)
				{
					case 801: return "Key"; //24
					case 810: return "Special Key"; //21

					default:
						if (ClientType >= 811 && ClientType <= 839)
							return "Special Key"; 
						return Group;
				}
			}

			if (ClientType >= 1000 && ClientType <= 1049)
			{
				Group = "Readables";

				switch (ClientType)
				{
					case 1001: return "Mage Spellbook"; //85
					case 1002: return "Cleric Spellbook"; //85
					case 1011: return "Armor Improver"; //123
					case 1016: return "Mage Spellbook"; //124
					case 1021: return "Skill Scroll"; //130
					case 1041: return "Book/Scroll"; //8

					default: return Group;
				}
			}

			if (ClientType >= 1100 && ClientType <= 1149)
			{
				Group = "Lights & Lightables";

				switch (ClientType)
				{
					case 1101: return "Lighter"; //75
					case 1102: return "Torch";
					case 1103: return "Colored Torch";

					default: return Group;
				}
			}

			if (ClientType >= 2000 && ClientType <= 2049)
			{
				Group = "Valuables";

				switch (ClientType)
				{
					case 2001: return "Money"; //36
					case 2005: return "Gold Nugget";
					case 2011: return "Gems"; //60
					case 2030: return "Jewelery"; //60 - chalice, crystball

					default: return Group;
				}
			}

			if (ClientType >= 8000 && ClientType <= 8999)
			{
				Group = "Misc";

				switch (ClientType)
				{
					case 8001: return "Clock"; //9
					case 8002: return "Furniture"; //15
					case 8003: return "Ten Kilo";
					case 8006: return "Bagpipe"; //24
					case 8011: return "Gravestone"; //38
					case 8012: return "Boulder";
					case 8013: return "Pillar";
					case 8015: return "Flowers";
					case 8020: return "Ice Cube";
					case 8021: return "Transforming Item";

					default: return Group;
				}
			}

			if (ClientType >= 25000 && ClientType <= 25999)
			{
				Group = "Appliable";

				switch (ClientType)
				{
					case 25011: return "Town Portal"; //66
					case 25012: return "Exit"; //Exits, doors, buildings - much more mundane exits(66)
					case 25021: return "Sign"; //Ordinary signs(98), Shop Inventory(150)
					case 25031: return "Postbox";
					case 25041: return "Slot Machine";
					case 25042: return "Trigger";  //Trigger(27), levers(93)
					case 25091: return "Bed to Reality"; //106

					default: return Group;
				}
			}

			return ClientType.ToString();
		}
    }
}

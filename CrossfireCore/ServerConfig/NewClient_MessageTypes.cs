using System.ComponentModel;

namespace CrossfireCore.ServerConfig
{
    public static partial class NewClient
    {
        public enum MsgTypes
        {
            None = 0,

            [Description("Books")]
            Book = 1,

            [Description("Cards")]
            Card = 2,

            [Description("Paper")]
            Paper = 3,

            [Description("Signs & Magic Mouths")]
            Sign = 4,

            [Description("Monuments")]
            Monument = 5,

            /// <summary>
            /// NPCs, magic ears, and altars
            /// </summary>
            [Description("Dialogs (Alter/NPC/Magic Ear)")]
            Dialog = 6,

            /// <summary>
            /// Message of the Day
            /// </summary>
            [Description("Message of the Day")]
            MOTD = 7,

            [Description("Administrative")]
            Admin = 8,

            [Description("Shops")]
            Shop = 9,

            /// <summary>
            /// Responses to commands, eg, who
            /// </summary>
            [Description("Command Responses")]
            Command = 10,

            /// <summary>
            /// Changes to attributes (stats, resistances, etc
            /// </summary>
            [Description("Changes to Attributes")]
            Attribute = 11,

            /// <summary>
            /// Messages related to skill use.
            /// </summary>
            [Description("Skill related Messages")]
            Skill = 12,

            /// <summary>
            /// Applying objects
            /// </summary>
            [Description("Apply results")]
            Apply = 13,

            /// <summary>
            /// Attack related messages
            /// </summary>
            [Description("Attack results")]
            Attack = 14,

            /// <summary>
            /// Communication between players
            /// </summary>
            [Description("Player communication")]
            Communication = 15,

            /// <summary>
            /// Spell related info
            /// </summary>
            [Description("Spell results")]
            Spell = 16,

            /// <summary>
            /// Item related information
            /// </summary>
            [Description("Item information")]
            Item = 17,

            /// <summary>
            /// Messages that don't go elsewhere
            /// </summary>
            [Description("Miscellaneous")]
            Misc = 18,

            /// <summary>
            /// Something bad is happening to the player
            /// </summary>
            [Description("Victim notifications")]
            Victim = 19,

            /// <summary>
            /// Client originated Messages
            /// </summary>
            [Description("Client generated messages")]
            Client = 20,
        }

        public const int SubTypeNone = 0;

        public enum MsgSubTypeBook
        {
            Clasp1 = 1,
            Clasp2 = 2,
            Elegant1 = 3,
            Elegant2 = 4,
            Quarto1 = 5,
            Quarto2 = 6,
            SpellEvoker = 7,
            SpellPrayer = 8,
            SpellPyro = 9,
            SpellSorcerer = 10,
            SpellSummoner = 11,
        }

        public enum MsgSubTypeCard
        {
            Simple1 = 1,
            Simple2 = 2,
            Simple3 = 3,
            Elegant1 = 4,
            Elegant2 = 5,
            Elegant3 = 6,
            Strange1 = 7,
            Strange2 = 8,
            Strange3 = 9,
            Money1 = 10,
            Money2 = 11,
            Money3 = 12,
        }

        public enum MsgSubTypePaper
        {
            Note1 = 1,
            Note2 = 2,
            Note3 = 3,
            LetterOld1 = 4,
            LetterOld2 = 5,
            LetterNew1 = 6,
            LetterNew2 = 7,
            Envelope1 = 8,
            Envelope2 = 9,
            ScrollOld1 = 10,
            ScrollOld2 = 11,
            ScrollNew1 = 12,
            ScrollNew2 = 13,
            ScrollMagic = 14,
        }

        public enum MsgSubTypeSign
        {
            Basic = 1,
            DirLeft = 2,
            DirRight = 3,
            DirBoth = 4,
            MagicMouth = 5,
        }

        public enum MsgSubTypeMonument
        {
            Stone1 = 1,
            Stone2 = 2,
            Stone3 = 3,
            Statue1 = 4,
            Statue2 = 5,
            Statue3 = 6,
            Gravestone1 = 7,
            Gravestone2 = 8,
            Gravestone3 = 9,
            Wall1 = 10,
            Wall2 = 11,
            Wall3 = 12,
        }

        public enum MsgSubTypeDialog
        {
            /// <summary>
            /// A Message From The Npc
            /// </summary>
            Npc = 1,

            /// <summary>
            /// A Message From An Altar
            /// </summary>
            Altar = 2,

            /// <summary>
            /// A Message from Magic Ear
            /// </summary>
            MagicEar = 3,
        }

        public enum MsgSubTypeAdmin
        {
            Rules = 1,
            News = 2,

            /// <summary>
            /// Player coming/going/death
            /// </summary>
            Player = 3,

            /// <summary>
            /// DM related admin actions
            /// </summary>
            DM = 4,

            /// <summary>
            /// Hiscore list
            /// </summary>
            HiScore = 5,

            /// <summary>
            /// load/save operations
            /// </summary>
            LoadSave = 6,

            /// <summary>
            /// login messages/errors
            /// </summary>
            Login = 7,

            /// <summary>
            /// version info
            /// </summary>
            Version = 8,

            /// <summary>
            /// Error on command, setup, etc
            /// </summary>
            Error = 9
        }

        public enum MsgSubTypeShop
        {
            /// <summary>
            /// Shop Listings - Inventory, What It Deals In.
            /// </summary>
            Listing = 1,

            /// <summary>
            /// Messages About Payment, Lack Of Funds.
            /// </summary>
            Payment = 2,

            /// <summary>
            /// Messages About Selling Items
            /// </summary>
            Sell = 3,

            /// <summary>
            /// Random Messages
            /// </summary>
            Misc = 4,
        }

        public enum MsgSubTypeCommand
        {
            Who = 1,
            Maps = 2,
            Body = 3,
            Malloc = 4,
            Weather = 5,
            Statistics = 6,

            /// <summary>
            /// Bowmode, Petmode, Applymode
            /// </summary>
            Config = 7,

            /// <summary>
            /// Generic Info: Resistances, Etc
            /// </summary>
            Info = 8,

            /// <summary>
            /// Quest Info
            /// </summary>
            Quests = 9,

            /// <summary>
            /// Various Debug Type Commands
            /// </summary>
            Debug = 10,

            /// <summary>
            /// Bad Syntax/Can't Use Command
            /// </summary>
            Error = 11,

            /// <summary>
            /// Successful Result From Command
            /// </summary>
            Success = 12,

            /// <summary>
            /// Failed Result From Command
            /// </summary>
            Failure = 13,

            /// <summary>
            /// Player Examining Something
            /// </summary>
            Examine = 14,

            /// <summary>
            /// Inventory Listing
            /// </summary>
            Inventory = 15,

            /// <summary>
            /// Help Related Information
            /// </summary>
            Help = 16,

            /// <summary>
            /// Dm Related Commands
            /// </summary>
            Dm = 17,

            /// <summary>
            /// Create A New Character
            /// </summary>
            Newplayer = 18,
        }

        public enum MsgSubTypeAttribute
        {
            /// <summary>
            /// Gained Attack Type
            /// </summary>
            AttacktypeGain = 1,

            /// <summary>
            /// Lost Attack Type
            /// </summary>
            AttacktypeLoss = 2,

            /// <summary>
            /// Gained Protection
            /// </summary>
            ProtectionGain = 3,

            /// <summary>
            /// Lost Protection
            /// </summary>
            ProtectionLoss = 4,

            /// <summary>
            /// A Change In The Movement Type Of The Player.
            /// </summary>
            Move = 5,

            /// <summary>
            /// Race-Related Changes.
            /// </summary>
            Race = 6,

            /// <summary>
            /// Start Of A Bad Effect To The Player.
            /// </summary>
            BadEffectStart = 7,

            /// <summary>
            /// End Of A Bad Effect.
            /// </summary>
            BadEffectEnd = 8,

            StatGain = 9,
            StatLoss = 10,
            LevelGain = 11,
            LevelLoss = 12,

            /// <summary>
            /// Start Of A Good Effect To The Player.
            /// </summary>
            GoodEffectStart = 13,

            /// <summary>
            /// End Of A Good Effect.
            /// </summary>
            GoodEffectEnd = 14,

            /// <summary>
            /// Changing God Info
            /// </summary>
            God = 15,
        }

        public enum MsgSubTypeSkill
        {

            /// <summary>
            /// Don't Have The Skill
            /// </summary>
            Missing = 1,

            /// <summary>
            /// Doing Something Wrong
            /// </summary>
            Error = 2,

            /// <summary>
            /// Successfully Used Skill
            /// </summary>
            Success = 3,

            /// <summary>
            /// Failure In Using Skill
            /// </summary>
            Failure = 4,

            /// <summary>
            /// Praying Related Messages
            /// </summary>
            Pray = 5,

            /// <summary>
            /// List Of Skills
            /// </summary>
            List = 6,
        }

        public enum MsgSubTypeApply
        {
            Error = 1,

            /// <summary>
            /// Unapply An Object
            /// </summary>
            Unapply = 2,

            /// <summary>
            /// Was Able To Apply Object
            /// </summary>
            Success = 3,

            /// <summary>
            /// Apply Ok, But No/Bad Result
            /// </summary>
            Failure = 4,

            /// <summary>
            /// Applied A Cursed Object (Bad)
            /// </summary>
            Cursed = 5,

            /// <summary>
            /// Have Activated A Trap
            /// </summary>
            Trap = 6,

            /// <summary>
            ///  Don't Have Body To Use Object
            /// </summary>
            Badbody = 7,

            /// <summary>
            /// Class/God Prohibiiton On Obj
            /// </summary>
            Prohibition = 8,

            /// <summary>
            /// Build Related Actions
            /// </summary>
            Build = 9,
        }

        public enum MsgSubTypeAttack
        {
            /// <summary>
            /// Player Hit Something Else
            /// </summary>
            DidHit = 1,

            /// <summary>
            /// Players Pet Hit Something Else
            /// </summary>
            PetHit = 2,

            /// <summary>
            /// Player Fumbled Attack
            /// </summary>
            Fumble = 3,

            /// <summary>
            /// Player Killed Something
            /// </summary>
            DidKill = 4,

            /// <summary>
            /// Pet Was Killed
            /// </summary>
            PetDied = 5,

            /// <summary>
            /// Keys Are Like Attacks, So..
            /// </summary>
            Nokey = 6,

            /// <summary>
            /// You Avoid Attacking
            /// </summary>
            Noattack = 7,

            /// <summary>
            /// Pushed A Friendly Player
            /// </summary>
            Pushed = 8,

            /// <summary>
            /// Attack Didn't Hit
            /// </summary>
            Miss = 9,
        }

        public enum MsgSubTypeCommunication
        {
            /// <summary>
            /// Random Event (Coin Toss)
            /// </summary>
            Random = 1,

            /// <summary>
            /// Player Says Something
            /// </summary>
            Say = 2,

            /// <summary>
            /// Player Me's A Message
            /// </summary>
            Me = 3,

            /// <summary>
            /// Player Tells Something
            /// </summary>
            Tell = 4,

            /// <summary>
            /// Player Emotes
            /// </summary>
            Emote = 5,

            /// <summary>
            /// Party Message
            /// </summary>
            Party = 6,

            /// <summary>
            /// Party Message
            /// </summary>
            Shout = 7,

            /// <summary>
            /// Party Message
            /// </summary>
            Chat = 8,
        }

        public enum MsgSubTypeSpell
        {
            /// <summary>
            /// Healing Related Spells
            /// </summary>
            Heal = 1,

            /// <summary>
            /// Pet Related Messages
            /// </summary>
            Pet = 2,

            /// <summary>
            /// Spell Failure Messages
            /// </summary>
            Failure = 3,

            /// <summary>
            /// A Spell Ends
            /// </summary>
            End = 4,

            /// <summary>
            /// Spell Succeeded Messages
            /// </summary>
            Success = 5,

            /// <summary>
            /// Spell Failure Messages
            /// </summary>
            Error = 6,

            /// <summary>
            /// Perceive Self Messages
            /// </summary>
            PerceiveSelf = 7,

            /// <summary>
            /// Target Of Non Attack Spell
            /// </summary>
            Target = 8,

            /// <summary>
            /// Random Info About Spell, Not Related To Failure/Success
            /// </summary>
            Info = 9,
        }

        public enum MsgSubTypeItem
        {
            /// <summary>
            /// Item Removed From Inv
            /// </summary>
            Remove = 1,

            /// <summary>
            /// Item Added To Inventory
            /// </summary>
            Add = 2,

            /// <summary>
            /// Item Has Changed In Some Way
            /// </summary>
            Change = 3,

            /// <summary>
            /// Information Related To Items
            /// </summary>
            Info = 4,
        }

        public enum MsgSubTypeVictim
        {
            /// <summary>
            /// Player Is Sinking In A Swamp
            /// </summary>
            Swamp = 1,

            /// <summary>
            /// Player Was Hit By Something
            /// </summary>
            WasHit = 2,

            /// <summary>
            /// Someone Tried To Steal From The Player
            /// </summary>
            Steal = 3,

            /// <summary>
            /// Someone Cast A Bad Spell On The Player
            /// </summary>
            Spell = 4,

            /// <summary>
            /// Player Died!
            /// </summary>
            Died = 5,

            /// <summary>
            /// Player Was Pushed Or Attempted Pushed
            /// </summary>
            WasPushed = 6,
        }

        public enum MsgSubTypeClient
        {
            /// <summary>
            /// Local Configuration Issues
            /// </summary>
            Config = 1,

            /// <summary>
            /// Server Configuration Issues
            /// </summary>
            Server = 2,

            /// <summary>
            /// Drawinfocmd()
            /// </summary>
            Command = 3,

            /// <summary>
            /// HandleQuery() And Prompts
            /// </summary>
            Query = 4,

            /// <summary>
            /// General Debug Messages
            /// </summary>
            Debug = 5,

            /// <summary>
            /// Non-Critical Note To Player
            /// </summary>
            Notice = 6,

            /// <summary>
            /// Metaserver Messages
            /// </summary>
            Metaserver = 7,

            /// <summary>
            /// Script Related Messages
            /// </summary>
            Script = 8,

            /// <summary>
            /// Bad Things Happening
            /// </summary>
            Error = 9,
        }
    }
}

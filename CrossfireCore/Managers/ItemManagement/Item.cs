using Common.Utility;
using CrossfireRPG.ServerInterface.Definitions;
using System;

namespace CrossfireCore.Managers.ItemManagement
{
    public class Item : DataObject
    {
        /// <summary>
        /// Tag of item
        /// </summary>
        public UInt32 Tag { get; set; }

        /// <summary>
        /// Tag of object that owns Item, 0 if on ground
        /// </summary>
        public UInt32 LocationTag { get => _LocationTag; set => SetProperty(ref _LocationTag, value); }
        private UInt32 _LocationTag;

        public NewClient.ItemFlags Flags { get => _Flags; set => SetProperty(ref _Flags, value); }
        private NewClient.ItemFlags _Flags;


        public NewClient.ItemFlags FlagsNoPick => Flags & ~NewClient.ItemFlags.NoPick;
        public NewClient.ItemFlags FlagsNoApply => Flags & ~NewClient.ItemFlags.Applied_Mask;
        public NewClient.ItemFlags FlagsNoApplyNoPick => Flags & ~(NewClient.ItemFlags.Applied_Mask | NewClient.ItemFlags.NoPick);


        /// <summary>
        /// Item weight in kg
        /// </summary>
        public float Weight { get => _Weight; set => SetProperty(ref _Weight, value,
            nameof(Weight), nameof(TotalWeight), nameof(HasWeight)); }
        private float _Weight;

        public UInt32 Face { get => _Face; set => SetProperty(ref _Face, value); }
        private UInt32 _Face;

        public string Name { get => _Name; set => SetProperty(ref _Name, value,
            nameof(Name), nameof(NameCase)); }
        private string _Name;

        public string NameCase => Name?.ToTitleCase() ?? "";

        public string NamePlural { get => _NamePlural; set => SetProperty(ref _NamePlural, value,
            nameof(NamePlural), nameof(NamePluralCase)); }
        private string _NamePlural;

        public string NamePluralCase => NamePlural?.ToTitleCase() ?? "";

        public UInt16 Animation { get => _Animation; set => SetProperty(ref _Animation, value); }
        private UInt16 _Animation;

        public byte AnimationSpeed { get => _AnimationSpeed; set => SetProperty(ref _AnimationSpeed, value); }
        private byte _AnimationSpeed;


        public UInt32 RawNumberOf { get => _RawNumberOf; set => SetProperty(ref _RawNumberOf, value,
            nameof(RawNumberOf), nameof(NumberOf), nameof(NumberOfInWords),
            nameof(NumberOfWithout1), nameof(NumberOfInWordsWithout1),
            nameof(TotalWeight)); }
        private UInt32 _RawNumberOf;


        /// <summary>
        /// Actual number of items, 1 or more
        /// </summary>
        public UInt32 NumberOf => RawNumberOf < 1 ? 1 : RawNumberOf;

        /// <summary>
        /// NumberOf in Words
        /// </summary>
        public string NumberOfInWords => NumberOf < _NumberWords.Length ? _NumberWords[NumberOf] : NumberOf.ToString();

        /// <summary>
        /// NumberOf, but a blank instead of "1"
        /// </summary>
        public string NumberOfWithout1 => NumberOf > 1 ? NumberOf.ToString() : "";

        /// <summary>
        /// NumberOf, but a blank instead of "One"
        /// </summary>
        public string NumberOfInWordsWithout1 => NumberOf > 1 ? NumberOfInWords : "";

        static string[] _NumberWords = new string[] { "None", "One", "Two", "Three", "Four", "Five",
            "Six", "Seven", "Eight", "Nine", "Ten" };

        public UInt16 ClientType { get => _ClientType; set => SetProperty(ref _ClientType, value); }
        private UInt16 _ClientType;

        /* From here, are new item properties and helper functions */

        /// <summary>
        /// Item weight in kg multiplied by NumberOf
        /// </summary>
        public float TotalWeight => !HasWeight ? float.NaN : Weight * NumberOf;

        /// <summary>
        /// Location of item (this does not test if in container or not)
        /// </summary>
        public ItemLocations Location { get => _Location; set => SetProperty(ref _Location, value); }
        private ItemLocations _Location;

        /// <summary>
        /// Check if item is in a container (Container may be on the player or on the ground)
        /// </summary>
        public bool IsInContainer { get => _IsInContainer; set => SetProperty(ref _IsInContainer, value); }
        private bool _IsInContainer;

        /// <summary>
        /// Item is on the ground, and ground only (this does not test for in a container on the ground)
        /// </summary>
        public bool IsOnGround => LocationTag == 0;
        public bool HasWeight => !float.IsNaN(Weight);
        public bool IsApplied => (Flags & NewClient.ItemFlags.Applied_Mask) > 0;
        public NewClient.ItemFlags ApplyType => Flags & NewClient.ItemFlags.Applied_Mask;
        public bool IsUnidentified => Flags.HasFlag(NewClient.ItemFlags.Unidentified);
        public bool IsUnpaid => Flags.HasFlag(NewClient.ItemFlags.Unpaid);
        public bool IsMagic => Flags.HasFlag(NewClient.ItemFlags.Magic);
        public bool IsCursed => Flags.HasFlag(NewClient.ItemFlags.Cursed);
        public bool IsDamned => Flags.HasFlag(NewClient.ItemFlags.Damned);
        public bool IsOpen => Flags.HasFlag(NewClient.ItemFlags.Open);
        public bool IsNoPick => Flags.HasFlag(NewClient.ItemFlags.NoPick);
        public bool IsLocked => Flags.HasFlag(NewClient.ItemFlags.Locked);
        public bool IsBlessed => Flags.HasFlag(NewClient.ItemFlags.Blessed);
        public bool IsRead => Flags.HasFlag(NewClient.ItemFlags.Read);

        public override string ToString()
        {
            return string.Format("Item: {0} (Tag={1} Face={2} Flags={3} Location={4} Number={5})",
                Name, Tag, Face, Flags, LocationTag, RawNumberOf);
        }

        public object[] FormatArgs => new object[]
        {
            NameCase,
            NamePluralCase,
            NumberOf,
            NumberOfInWords,
            NumberOfWithout1,
            NumberOfInWordsWithout1,
            Weight,
            TotalWeight,
            Tag,
            LocationTag,
            Flags,
            FlagsNoPick,
            ClientTypes.GetClientTypeInfo(ClientType, out var clientGroup),
        };

        public const string FormatHelp = "{0}=Name {1}=PluralName {2}=NumberOf {3}=NumberOfInWords " +
            "{4}=NumberOfWithout1 {5}=NumberOfInWordsWithout1 {6}=Weight {7}=TotalWeight {8}=Tag " +
            "{9}=Location {10}=Flags {11}=FlagsNoPick {12}=ClientType";
    }
}

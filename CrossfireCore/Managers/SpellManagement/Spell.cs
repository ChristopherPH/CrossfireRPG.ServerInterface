using System;

namespace CrossfireRPG.ServerInterface.Managers.SpellManagement
{
    public class Spell : DataObject
    {
        public UInt32 SpellTag { get; set; }
        public Int16 Level { get; set; }
        public Int16 CastingTime { get; set; }

        public Int16 Mana { get => _Mana; set => SetProperty(ref _Mana, value); }
        private Int16 _Mana;

        public Int16 Grace { get => _Grace; set => SetProperty(ref _Grace, value); }
        private Int16 _Grace;

        public Int16 Damage { get => _Damage; set => SetProperty(ref _Damage, value); }
        private Int16 _Damage;

        public byte Skill { get; set; }
        public UInt32 Path { get; set; }
        public Int32 Face { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// 0: spell needs no argument.
        /// 1: spell needs the name of another spell.
        /// 2: spell can use a freeform string argument.
        /// 3: spell requires a freeform string argument.
        /// </summary>
        public byte Usage { get; set; } = 0;

        /// <summary>
        /// Comma-separated list of items required to cast spell,
        /// potential number of items, singular names
        /// </summary>
        public string Requirements { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}

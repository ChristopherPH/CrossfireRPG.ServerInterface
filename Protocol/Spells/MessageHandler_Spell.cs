/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using CrossfireRPG.ServerInterface.Definitions;
using System;

namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageHandler
    {
        public event EventHandler<AddSpellEventArgs> AddSpell;
        public event EventHandler<UpdateSpellEventArgs> UpdateSpell;
        public event EventHandler<DeleteSpellEventArgs> DeleteSpell;

        protected override void HandleAddSpell(UInt32 SpellTag, Int16 Level, Int16 CastingTime,
            Int16 Mana, Int16 Grace, Int16 Damage, byte Skill, UInt32 Path, UInt32 Face,
            string Name, string Description, byte Usage, string Requirements)
        {
            AddSpell?.Invoke(this, new AddSpellEventArgs()
            {
                SpellTag = SpellTag,
                Level = Level,
                CastingTime = CastingTime,
                Mana = Mana,
                Grace = Grace,
                Damage = Damage,
                Skill = Skill,
                Path = Path,
                Face = Face,
                Name = Name,
                Description = Description,
                Usage = Usage,
                Requirements = Requirements
            });
        }

        protected override void HandleUpdateSpell(UInt32 SpellTag, NewClient.UpdateSpellTypes UpdateType, Int64 UpdateValue)
        {
            UpdateSpell?.Invoke(this, new UpdateSpellEventArgs()
            {
                SpellTag = SpellTag,
                UpdateType = UpdateType,
                UpdateValue = UpdateValue
            });
        }

        protected override void HandleDeleteSpell(UInt32 SpellTag)
        {
            DeleteSpell?.Invoke(this, new DeleteSpellEventArgs()
            {
                SpellTag = SpellTag,
            });
        }

        public class AddSpellEventArgs : BatchEventArgs
        {
            public UInt32 SpellTag { get; set; }
            public Int16 Level { get; set; }
            public Int16 CastingTime { get; set; }
            public Int16 Mana { get; set; }
            public Int16 Grace { get; set; }
            public Int16 Damage { get; set; }
            public byte Skill { get; set; }
            public UInt32 Path { get; set; }
            public UInt32 Face { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public byte Usage { get; set; }
            public string Requirements { get; set; }
        }

        public class UpdateSpellEventArgs : BatchEventArgs
        {
            public UInt32 SpellTag { get; set; }
            public NewClient.UpdateSpellTypes UpdateType { get; set; }
            public Int64 UpdateValue { get; set; }
        }

        public class DeleteSpellEventArgs : MessageHandlerEventArgs
        {
            public UInt32 SpellTag { get; set; }
        }
    }
}

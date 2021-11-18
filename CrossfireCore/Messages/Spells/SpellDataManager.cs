﻿using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossfireCore.Managers
{
    public class SpellDataManager : DataListManager<Spell>
    {
        public SpellDataManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Handler.AddSpell += Handler_AddSpell;
            Handler.UpdateSpell += Handler_UpdateSpell;
            Handler.DeleteSpell += Handler_DeleteSpell;
        }

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => true;
        public override ModificationTypes SupportedModificationTypes => base.SupportedModificationTypes | 
            ModificationTypes.Added | ModificationTypes.Updated | ModificationTypes.Removed;

        private void Handler_AddSpell(object sender, MessageHandler.AddSpellEventArgs e)
        {
            AddData(new Spell()
            {
                SpellTag = e.SpellTag,
                Level = e.Level,
                CastingTime = e.CastingTime,
                Mana = e.Mana,
                Grace = e.Grace,
                Damage = e.Damage,
                Skill = e.Skill,
                Path = e.Path,
                Face = e.Face,
                Name = e.Name,
                Description = e.Description,
                Usage = e.Usage,
                Requirements = e.Requirements,
            });
        }

        private void Handler_UpdateSpell(object sender, MessageHandler.UpdateSpellEventArgs e)
        {
            UpdateData(x => x.SpellTag == e.SpellTag, (data) =>
            {
                switch (e.UpdateType)
                {
                    case NewClient.UpdateSpellTypes.Mana:
                        data.Mana = (short)e.UpdateValue;
                        return new string[] { nameof(Spell.Mana) };

                    case NewClient.UpdateSpellTypes.Grace:
                        data.Grace = (short)e.UpdateValue;
                        return new string[] { nameof(Spell.Grace) };

                    case NewClient.UpdateSpellTypes.Damage:
                        data.Damage = (short)e.UpdateValue;
                        return new string[] { nameof(Spell.Damage) };

                    default:
                        return null;
                }
            });
        }

        private void Handler_DeleteSpell(object sender, MessageHandler.DeleteSpellEventArgs e)
        {
            RemoveData(x => x.SpellTag == e.SpellTag);
        }
    }

    public class Spell
    {
        public UInt32 SpellTag { get; set; }
        public Int16 Level { get; set; }
        public Int16 CastingTime { get; set; }
        public Int16 Mana { get; set; }
        public Int16 Grace { get; set; }
        public Int16 Damage { get; set; }
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
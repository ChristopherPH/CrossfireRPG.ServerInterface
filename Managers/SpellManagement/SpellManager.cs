﻿/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using CrossfireRPG.ServerInterface.Definitions;
using CrossfireRPG.ServerInterface.Network;
using CrossfireRPG.ServerInterface.Protocol;
using System;
using System.Collections.Generic;

namespace CrossfireRPG.ServerInterface.Managers.SpellManagement
{
    public class SpellManager : DataListManager<UInt32, Spell, List<Spell>>
    {
        public SpellManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Handler.AddSpell += Handler_AddSpell;
            Handler.UpdateSpell += Handler_UpdateSpell;
            Handler.DeleteSpell += Handler_DeleteSpell;

            Handler.BeginAddSpell += Handler_BeginAddSpell;
            Handler.EndAddSpell += Handler_EndAddSpell;
        }

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => true;
        public override DataModificationTypes SupportedModificationTypes => base.SupportedModificationTypes | 
            DataModificationTypes.Added | DataModificationTypes.Updated | DataModificationTypes.Removed |
            DataModificationTypes.MultiCommandStart | DataModificationTypes.MultiCommandEnd;

        private void Handler_AddSpell(object sender, MessageHandler.AddSpellEventArgs e)
        {
            AddDataObject(e.SpellTag, new Spell()
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
            UpdateDataObject(e.SpellTag, (data) =>
            {
                if (e.UpdateTypes.HasFlag(NewClient.UpdateSpellTypes.Mana))
                    data.Mana = e.Mana;

                if (e.UpdateTypes.HasFlag(NewClient.UpdateSpellTypes.Grace))
                    data.Grace = e.Grace;

                if (e.UpdateTypes.HasFlag(NewClient.UpdateSpellTypes.Damage))
                    data.Damage = e.Damage;
            });
        }

        private void Handler_DeleteSpell(object sender, MessageHandler.DeleteSpellEventArgs e)
        {
            RemoveDataObject(e.SpellTag);
        }


        private void Handler_BeginAddSpell(object sender, BeginBatchEventArgs e)
        {
            StartMultiCommand();
        }


        private void Handler_EndAddSpell(object sender, EndBatchEventArgs e)
        {
            EndMultiCommand();
        }
    }
}

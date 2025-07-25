﻿/*
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
    public partial class MessageParser
    {
        protected abstract void HandleAddSpell(UInt32 SpellTag, Int16 Level, Int16 CastingTime, Int16 Mana, Int16 Grace,
            Int16 Damage, byte Skill, UInt32 Path, UInt32 Face, string Name, string Description, byte Usage,
            string Requirements);
        protected abstract void HandleBeginAddSpell();
        protected abstract void HandleEndAddSpell();

        protected abstract void HandleUpdateSpell(UInt32 SpellTag, NewClient.UpdateSpellTypes UpdateTypes,
            Int16 Mana, Int16 Grace, Int16 Damage);
        protected abstract void HandleDeleteSpell(UInt32 SpellTag);

        //Save the spellmon value for the parser
        private int ParserOption_SpellMon = 0;

        private void AddSpellParsers()
        {
            AddCommandHandler("addspell", new CommandParserDefinition(Parse_addspell));
            AddCommandHandler("updspell", new CommandParserDefinition(Parse_updspell));
            AddCommandHandler("delspell", new CommandParserDefinition(Parse_delspell));
        }

        private bool Parse_addspell(byte[] Message, ref int DataOffset, int DataEnd)
        {
            HandleBeginAddSpell();

            while (DataOffset < DataEnd)
            {
                var spell_tag = BufferTokenizer.GetUInt32(Message, ref DataOffset);
                var spell_level = BufferTokenizer.GetInt16(Message, ref DataOffset);
                var spell_cast_time = BufferTokenizer.GetInt16(Message, ref DataOffset);
                var spell_mana = BufferTokenizer.GetInt16(Message, ref DataOffset);
                var spell_grace = BufferTokenizer.GetInt16(Message, ref DataOffset);
                var spell_damage = BufferTokenizer.GetInt16(Message, ref DataOffset);
                var spell_skill = BufferTokenizer.GetByte(Message, ref DataOffset);
                var spell_path = BufferTokenizer.GetUInt32(Message, ref DataOffset);
                var spell_face = BufferTokenizer.GetUInt32(Message, ref DataOffset);
                var spell_name_len = BufferTokenizer.GetByte(Message, ref DataOffset);
                var spell_name = BufferTokenizer.GetBytesAsString(Message, ref DataOffset, spell_name_len);
                var spell_desc_len = BufferTokenizer.GetInt16(Message, ref DataOffset);
                var spell_desc = BufferTokenizer.GetBytesAsString(Message, ref DataOffset, spell_desc_len);

                byte spell_usage = 0;
                string spell_requirement = string.Empty;

                //extra data for spellmon 2
                if (ParserOption_SpellMon > 1)
                {
                    spell_usage = BufferTokenizer.GetByte(Message, ref DataOffset);
                    var spell_requirement_len = BufferTokenizer.GetByte(Message, ref DataOffset);

                    if (spell_requirement_len != 0)
                        spell_requirement = BufferTokenizer.GetBytesAsString(Message, ref DataOffset, spell_requirement_len);
                }

                HandleAddSpell(spell_tag, spell_level, spell_cast_time, spell_mana, spell_grace, spell_damage, spell_skill,
                    spell_path, spell_face, spell_name, spell_desc, spell_usage, spell_requirement);
            }

            HandleEndAddSpell();

            return true;
        }

        private bool Parse_updspell(byte[] Message, ref int DataOffset, int DataEnd)
        {
            Int16 mana = 0, grace = 0, damage = 0;
            var update_spell_flag = (NewClient.UpdateSpellTypes)BufferTokenizer.GetByte(Message, ref DataOffset);
            var update_spell_tag = BufferTokenizer.GetUInt32(Message, ref DataOffset);

            if (update_spell_flag.HasFlag(NewClient.UpdateSpellTypes.Mana))
                mana = BufferTokenizer.GetInt16(Message, ref DataOffset);

            if (update_spell_flag.HasFlag(NewClient.UpdateSpellTypes.Grace))
                grace = BufferTokenizer.GetInt16(Message, ref DataOffset);

            if (update_spell_flag.HasFlag(NewClient.UpdateSpellTypes.Damage))
                damage = BufferTokenizer.GetInt16(Message, ref DataOffset);

            HandleUpdateSpell(update_spell_tag, update_spell_flag, mana, grace, damage);

            return true;
        }

        private bool Parse_delspell(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var del_spell_tag = BufferTokenizer.GetUInt32(Message, ref DataOffset);

            HandleDeleteSpell(del_spell_tag);
            
            return true;
        }
    }
}

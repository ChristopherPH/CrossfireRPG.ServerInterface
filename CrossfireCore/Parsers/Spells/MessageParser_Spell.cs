using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParserBase
    {
        protected abstract void HandleAddSpell(UInt32 SpellTag, Int16 Level, Int16 CastingTime, Int16 Mana, Int16 Grace,
            Int16 Damage, byte Skill, UInt32 Path, Int32 Face, string Name, string Description, byte Usage,
            string Requirements);

        protected abstract void HandleUpdateSpell(UInt32 SpellTag, NewClient.UpdateSpellTypes UpdateType, Int64 UpdateValue);
        protected abstract void HandleDeleteSpell(UInt32 SpellTag);

        private void AddSpellParsers()
        {
            AddCommandHandler("addspell", Parse_addspell);
            AddCommandHandler("updspell", Parse_updspell);
            AddCommandHandler("delspell", Parse_delspell);
        }

        private bool Parse_addspell(byte[] packet, ref int offset)
        {
            while (offset < packet.Length)
            {
                var spell_tag = BufferTokenizer.GetUInt32(packet, ref offset);
                var spell_level = BufferTokenizer.GetInt16(packet, ref offset);
                var spell_cast_time = BufferTokenizer.GetInt16(packet, ref offset);
                var spell_mana = BufferTokenizer.GetInt16(packet, ref offset);
                var spell_grace = BufferTokenizer.GetInt16(packet, ref offset);
                var spell_damage = BufferTokenizer.GetInt16(packet, ref offset);
                var spell_skill = BufferTokenizer.GetByte(packet, ref offset);
                var spell_path = BufferTokenizer.GetUInt32(packet, ref offset);
                var spell_face = BufferTokenizer.GetInt32(packet, ref offset);
                var spell_name_len = BufferTokenizer.GetByte(packet, ref offset);
                var spell_name = BufferTokenizer.GetBytesAsString(packet, ref offset, spell_name_len);
                var spell_desc_len = BufferTokenizer.GetInt16(packet, ref offset);
                var spell_desc = BufferTokenizer.GetBytesAsString(packet, ref offset, spell_desc_len);

                byte spell_usage = 0;
                string spell_requirement = string.Empty;

                //extra data for spellmon 2
                if (ParserOption_SpellMon > 1)
                {
                    spell_usage = BufferTokenizer.GetByte(packet, ref offset);
                    var spell_requirement_len = BufferTokenizer.GetByte(packet, ref offset);
                    spell_requirement = BufferTokenizer.GetBytesAsString(packet, ref offset, spell_requirement_len);
                }

                HandleAddSpell(spell_tag, spell_level, spell_cast_time, spell_mana, spell_grace, spell_damage, spell_skill,
                    spell_path, spell_face, spell_name, spell_desc, spell_usage, spell_requirement);
            }

            return true;
        }

        private bool Parse_updspell(byte[] packet, ref int offset)
        {
            var update_spell_flag = (NewClient.UpdateSpellTypes)BufferTokenizer.GetByte(packet, ref offset);
            var update_spell_tag = BufferTokenizer.GetUInt32(packet, ref offset);

            while (offset < packet.Length)
            {
                if (update_spell_flag.HasFlag(NewClient.UpdateSpellTypes.Mana))
                {
                    var update_spell_value = BufferTokenizer.GetInt16(packet, ref offset);
                    HandleUpdateSpell(update_spell_tag, update_spell_flag, update_spell_value);
                }

                if (update_spell_flag.HasFlag(NewClient.UpdateSpellTypes.Grace))
                {
                    var update_spell_value = BufferTokenizer.GetInt16(packet, ref offset);
                    HandleUpdateSpell(update_spell_tag, update_spell_flag, update_spell_value);
                }

                if (update_spell_flag.HasFlag(NewClient.UpdateSpellTypes.Damage))
                {
                    var update_spell_value = BufferTokenizer.GetInt16(packet, ref offset);
                    HandleUpdateSpell(update_spell_tag, update_spell_flag, update_spell_value);
                }
            }

            return true;
        }

        private bool Parse_delspell(byte[] packet, ref int offset)
        {
            var del_spell_tag = BufferTokenizer.GetUInt32(packet, ref offset);

            HandleDeleteSpell(del_spell_tag);
            
            return true;
        }
    }
}

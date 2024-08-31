using CrossfireRPG.ServerInterface.Definitions;
using System;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParser
    {
        protected abstract void HandleAddSpell(UInt32 SpellTag, Int16 Level, Int16 CastingTime, Int16 Mana, Int16 Grace,
            Int16 Damage, byte Skill, UInt32 Path, Int32 Face, string Name, string Description, byte Usage,
            string Requirements);

        protected abstract void HandleUpdateSpell(UInt32 SpellTag, NewClient.UpdateSpellTypes UpdateType, Int64 UpdateValue);
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
                var spell_face = BufferTokenizer.GetInt32(Message, ref DataOffset);
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

            return true;
        }

        private bool Parse_updspell(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var update_spell_flag = (NewClient.UpdateSpellTypes)BufferTokenizer.GetByte(Message, ref DataOffset);
            var update_spell_tag = BufferTokenizer.GetUInt32(Message, ref DataOffset);

            while (DataOffset < DataEnd)
            {
                if (update_spell_flag.HasFlag(NewClient.UpdateSpellTypes.Mana))
                {
                    var update_spell_value = BufferTokenizer.GetInt16(Message, ref DataOffset);
                    HandleUpdateSpell(update_spell_tag, update_spell_flag, update_spell_value);
                }

                if (update_spell_flag.HasFlag(NewClient.UpdateSpellTypes.Grace))
                {
                    var update_spell_value = BufferTokenizer.GetInt16(Message, ref DataOffset);
                    HandleUpdateSpell(update_spell_tag, update_spell_flag, update_spell_value);
                }

                if (update_spell_flag.HasFlag(NewClient.UpdateSpellTypes.Damage))
                {
                    var update_spell_value = BufferTokenizer.GetInt16(Message, ref DataOffset);
                    HandleUpdateSpell(update_spell_tag, update_spell_flag, update_spell_value);
                }
            }

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

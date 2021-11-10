using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParserBase
    {
        protected abstract void HandleAddKnowledge(UInt32 ID, string Type, string Title, Int32 Face);
        protected abstract void HandleAddQuest(UInt32 Code, string Title, Int32 Face, byte Replay, UInt32 Parent,
            byte End, string Step);
        protected abstract void HandleUpdateQuest(UInt32 Code, byte End, string Step);

        private void AddQuestParsers()
        {
            AddCommandHandler("addknowledge", new CommandParserDefinition(Parse_addknowledge));
            AddCommandHandler("addquest", new CommandParserDefinition(Parse_addquest));
            AddCommandHandler("updquest", new CommandParserDefinition(Parse_updquest));
        }

        private bool Parse_addknowledge(byte[] packet, ref int offset, int end)
        {
            while (offset < end)
            {
                var knowledge_id = BufferTokenizer.GetUInt32(packet, ref offset);
                var knowledge_type_len = BufferTokenizer.GetUInt16(packet, ref offset);
                var knowledge_type = BufferTokenizer.GetBytesAsString(packet, ref offset, knowledge_type_len);
                var knowledge_title_len = BufferTokenizer.GetUInt16(packet, ref offset);
                var knowledge_title = BufferTokenizer.GetBytesAsString(packet, ref offset, knowledge_title_len);
                var knowledge_face = BufferTokenizer.GetInt32(packet, ref offset);

                HandleAddKnowledge(knowledge_id, knowledge_type, knowledge_title, knowledge_face);
            }

            return true;
        }

        private bool Parse_addquest(byte[] packet, ref int offset, int end)
        {
            while (offset < end)
            {
                var quest_code = BufferTokenizer.GetUInt32(packet, ref offset);
                var quest_title_len = BufferTokenizer.GetUInt16(packet, ref offset);
                var quest_title = BufferTokenizer.GetBytesAsString(packet, ref offset, quest_title_len);
                var quest_face = BufferTokenizer.GetInt32(packet, ref offset);
                var quest_replay = BufferTokenizer.GetByte(packet, ref offset);
                var quest_parent_code = BufferTokenizer.GetUInt32(packet, ref offset);
                var quest_end = BufferTokenizer.GetByte(packet, ref offset);
                var quest_step_len = BufferTokenizer.GetUInt16(packet, ref offset);
                var quest_step = BufferTokenizer.GetBytesAsString(packet, ref offset, quest_step_len);

                HandleAddQuest(quest_code, quest_title, quest_face, quest_replay, quest_parent_code,
                    quest_end, quest_step);
            }

            return true;
        }

        private bool Parse_updquest(byte[] packet, ref int offset, int end)
        {
            var update_quest_code = BufferTokenizer.GetUInt32(packet, ref offset);
            var update_quest_end = BufferTokenizer.GetByte(packet, ref offset);
            var update_quest_step_len = BufferTokenizer.GetUInt16(packet, ref offset);
            var update_quest_step = BufferTokenizer.GetBytesAsString(packet, ref offset, update_quest_step_len);

            HandleUpdateQuest(update_quest_code, update_quest_end, update_quest_step);

            return true;
        }
    }
}

using System;

namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageParser
    {
        protected abstract void HandleAddQuest(UInt32 Code, string Title, Int32 Face, byte Replay, UInt32 Parent,
            byte End, string Step);
        protected abstract void HandleUpdateQuest(UInt32 Code, byte End, string Step);
        protected abstract void HandleBeginQuests();
        protected abstract void HandleEndQuests();

        private void AddQuestParsers()
        {
            AddCommandHandler("addquest", new CommandParserDefinition(Parse_addquest));
            AddCommandHandler("updquest", new CommandParserDefinition(Parse_updquest));
        }

        private bool Parse_addquest(byte[] Message, ref int DataOffset, int DataEnd)
        {
            HandleBeginQuests();

            while (DataOffset < DataEnd)
            {
                var quest_code = BufferTokenizer.GetUInt32(Message, ref DataOffset);
                var quest_title_len = BufferTokenizer.GetUInt16(Message, ref DataOffset);
                var quest_title = BufferTokenizer.GetBytesAsString(Message, ref DataOffset, quest_title_len);
                var quest_face = BufferTokenizer.GetInt32(Message, ref DataOffset);
                var quest_replay = BufferTokenizer.GetByte(Message, ref DataOffset);
                var quest_parent_code = BufferTokenizer.GetUInt32(Message, ref DataOffset);
                var quest_end = BufferTokenizer.GetByte(Message, ref DataOffset);
                var quest_step_len = BufferTokenizer.GetUInt16(Message, ref DataOffset);
                var quest_step = BufferTokenizer.GetBytesAsString(Message, ref DataOffset, quest_step_len);

                HandleAddQuest(quest_code, quest_title, quest_face, quest_replay, quest_parent_code,
                    quest_end, quest_step);
            }

            HandleEndQuests();

            return true;
        }

        private bool Parse_updquest(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var update_quest_code = BufferTokenizer.GetUInt32(Message, ref DataOffset);
            var update_quest_end = BufferTokenizer.GetByte(Message, ref DataOffset);
            var update_quest_step_len = BufferTokenizer.GetUInt16(Message, ref DataOffset);
            var update_quest_step = BufferTokenizer.GetBytesAsString(Message, ref DataOffset, update_quest_step_len);

            HandleUpdateQuest(update_quest_code, update_quest_end, update_quest_step);

            return true;
        }
    }
}

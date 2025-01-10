/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System;

namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageParser
    {
        protected abstract void HandleAddQuest(UInt32 Code, string Title, Int32 Face, byte Replay, UInt32 Parent,
            byte End, string Description, string Step);
        protected abstract void HandleUpdateQuest(UInt32 Code, byte End, string Step);
        protected abstract void HandleBeginQuests();
        protected abstract void HandleEndQuests();

        //Save the notification value for the parser
        private int ParserOption_Notifications = 0;

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

                var quest_description = string.Empty;

                if (ParserOption_Notifications >= 4)
                {
                    /* added in setup notifications 4 */
                    var quest_desc_len = BufferTokenizer.GetUInt16(Message, ref DataOffset);
                    quest_description = BufferTokenizer.GetBytesAsString(Message, ref DataOffset, quest_desc_len);
                }

                HandleAddQuest(quest_code, quest_title, quest_face, quest_replay, quest_parent_code,
                    quest_end, quest_description, quest_step);
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

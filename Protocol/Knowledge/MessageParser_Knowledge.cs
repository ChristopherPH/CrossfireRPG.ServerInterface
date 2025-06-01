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
        protected abstract void HandleAddKnowledge(UInt32 ID, string Type, string Title, UInt32 Face);

        private void AddKnowledgeParsers()
        {
            AddCommandHandler("addknowledge", new CommandParserDefinition(Parse_addknowledge));
        }

        private bool Parse_addknowledge(byte[] Message, ref int DataOffset, int DataEnd)
        {
            while (DataOffset < DataEnd)
            {
                var knowledge_id = BufferTokenizer.GetUInt32(Message, ref DataOffset);
                var knowledge_type_len = BufferTokenizer.GetUInt16(Message, ref DataOffset);
                var knowledge_type = BufferTokenizer.GetBytesAsString(Message, ref DataOffset, knowledge_type_len);
                var knowledge_title_len = BufferTokenizer.GetUInt16(Message, ref DataOffset);
                var knowledge_title = BufferTokenizer.GetBytesAsString(Message, ref DataOffset, knowledge_title_len);
                var knowledge_face = BufferTokenizer.GetUInt32(Message, ref DataOffset);

                HandleAddKnowledge(knowledge_id, knowledge_type, knowledge_title, knowledge_face);
            }

            return true;
        }
    }
}

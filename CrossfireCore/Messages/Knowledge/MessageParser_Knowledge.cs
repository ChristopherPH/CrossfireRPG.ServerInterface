using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParser
    {
        protected abstract void HandleAddKnowledge(UInt32 ID, string Type, string Title, Int32 Face);

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
                var knowledge_face = BufferTokenizer.GetInt32(Message, ref DataOffset);

                HandleAddKnowledge(knowledge_id, knowledge_type, knowledge_title, knowledge_face);
            }

            return true;
        }
    }
}

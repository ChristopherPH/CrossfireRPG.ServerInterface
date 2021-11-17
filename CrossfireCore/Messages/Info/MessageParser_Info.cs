using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParser
    {
        protected abstract void HandleReplyInfo(string request, byte[] reply);

        private void AddInfoParsers()
        {
            AddCommandHandler("replyinfo", new CommandParserDefinition(Parse_replyinfo));
        }

        private bool Parse_replyinfo(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var reply_info = BufferTokenizer.GetString(Message, ref DataOffset, DataEnd, BufferTokenizer.SpaceNewlineSeperator);
            var reply_bytes = BufferTokenizer.GetRemainingBytes(Message, ref DataOffset, DataEnd);
            HandleReplyInfo(reply_info, reply_bytes);

            //TODO: bring in replyinfo object into this message parser

            return true;
        }
    }
}

/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageParser
    {
        /// <summary>
        /// Minimum protocol version to use RequestInfo command
        /// </summary>
        public const int ServerProtocolVersionRequestInfoCommand = 1023;

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

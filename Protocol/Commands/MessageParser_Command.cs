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
        protected abstract void HandleStartedCommand(ushort startc_packet);
        protected abstract void HandleCompletedCommand(UInt16 comc_packet, UInt32 comc_time);
        protected abstract void HandleQuery(int Flags, string QueryText);

        private void AddCommandParsers()
        {
            AddCommandHandler("startc", new CommandParserDefinition(Parse_startc));
            AddCommandHandler("comc", new CommandParserDefinition(Parse_comc));
            AddCommandHandler("query", new CommandParserDefinition(Parse_query));
        }

        private bool Parse_startc(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var startc_packet = BufferTokenizer.GetUInt16(Message, ref DataOffset);

            HandleStartedCommand(startc_packet);

            return true;
        }

        private bool Parse_comc(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var comc_packet = BufferTokenizer.GetUInt16(Message, ref DataOffset);
            var comc_time = BufferTokenizer.GetUInt32(Message, ref DataOffset);
            //TODO: convert completed command time as it isn't quite correct

            HandleCompletedCommand(comc_packet, comc_time);

            return true;
        }

        private bool Parse_query(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var query_flags = BufferTokenizer.GetStringAsInt(Message, ref DataOffset, DataEnd);
            var query_text = BufferTokenizer.GetRemainingBytesAsString(Message, ref DataOffset, DataEnd);

            HandleQuery(query_flags, query_text);

            return true;
        }
    }
}

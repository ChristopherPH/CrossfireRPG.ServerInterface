/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using Common.Logging;

namespace CrossfireRPG.ServerInterface.Protocol
{
    public class CommandParserDefinition
    {
        public delegate bool CommandParser(byte[] Message, ref int DataOffset, int DataEnd);

        public CommandParserDefinition() { }

        public CommandParserDefinition(CommandParser Parser)
        {
            this.Parser = Parser;
        }

        public CommandParserDefinition(CommandParser Parser, Logger.Levels Level)
        {
            this.Parser = Parser;
            this.Level = Level;
        }

        public CommandParser Parser { get; set; }
        public Logger.Levels Level { get; set; } = Logger.Levels.Info;
    }
}

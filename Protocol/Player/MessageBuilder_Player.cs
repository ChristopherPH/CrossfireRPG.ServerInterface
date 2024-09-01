/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System.Collections.Generic;

namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageBuilder
    {
        public bool SendProtocolCreatePlayer(string UserName, string Password)
        {
            using (var ba = new BufferAssembler("createplayer"))
            {
                ba.AddLengthPrefixedString(UserName);
                ba.AddLengthPrefixedString(Password);

                return SendProtocolMessage(ba);
            }
        }

        public bool SendProtocolCreatePlayer(string UserName, string Password,
            string RaceArch, string ClassArch, IEnumerable<KeyValuePair<string, int>> Stats,
            string StartingMapArch, IEnumerable<KeyValuePair<string, string>> Choices)
        {
            using (var ba = new BufferAssembler("createplayer"))
            {
                Logger.Debug("Create Player: {0}", UserName);
                ba.AddLengthPrefixedString(UserName);

                //_Logger.Debug("Create Player: {0}", Password);
                ba.AddLengthPrefixedString(Password);

                if (!string.IsNullOrWhiteSpace(RaceArch))
                {
                    Logger.Debug("Create Player: {0}", RaceArch);
                    ba.AddLengthPrefixedString("race {0}\0", RaceArch);
                }

                if (!string.IsNullOrWhiteSpace(ClassArch))
                {
                    Logger.Debug("Create Player: {0}", ClassArch);
                    ba.AddLengthPrefixedString("class {0}\0", ClassArch);
                }

                if (Stats != null)
                {
                    foreach (var stat in Stats)
                    {
                        if (!string.IsNullOrWhiteSpace(stat.Key))
                        {
                            Logger.Debug("Create Player: {0} {1}", stat.Key, stat.Value);
                            ba.AddLengthPrefixedString("{0} {1}\0",
                                stat.Key, stat.Value);
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(StartingMapArch))
                {
                    Logger.Debug("Create Player: {0}", StartingMapArch);
                    ba.AddLengthPrefixedString("starting_map {0}\0", StartingMapArch);
                }

                if (Choices != null)
                {
                    foreach (var choice in Choices)
                    {
                        if (!string.IsNullOrWhiteSpace(choice.Key) &&
                            !string.IsNullOrWhiteSpace(choice.Value))
                        {
                            Logger.Debug("Create Player: choice {0} {1}", choice.Key, choice.Value);
                            ba.AddLengthPrefixedString("choice {0} {1}\0",
                                choice.Key, choice.Value);
                        }
                    }
                }

                return SendProtocolMessage(ba);
            }
        }
    }
}

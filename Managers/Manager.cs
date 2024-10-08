﻿/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using CrossfireRPG.ServerInterface.Network;
using CrossfireRPG.ServerInterface.Protocol;

namespace CrossfireRPG.ServerInterface.Managers
{
    /// <summary>
    /// Base class for a data manager, used to hold, manage and organize data from the server
    /// </summary>
    public abstract class Manager
    {
        public Manager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
        {
            this.Connection = Connection;
            this.Builder = Builder;
            this.Handler = Handler;
        }

        /// <summary>
        /// Current connection to the server
        /// </summary>
        protected SocketConnection Connection { get; private set; }

        /// <summary>
        /// Builder for sending messages to the server
        /// </summary>
        protected MessageBuilder Builder { get; private set; }

        /// <summary>
        /// Parser for receiving messages from the server
        /// </summary>
        protected MessageHandler Handler { get; private set; }
    }
}

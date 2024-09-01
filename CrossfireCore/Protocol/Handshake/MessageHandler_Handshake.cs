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
    public partial class MessageHandler
    {
        public event EventHandler<EventArgs> AddmeFailed;
        public event EventHandler<EventArgs> AddmeSuccess;
        public event EventHandler<EventArgs> Goodbye;
        public event EventHandler<SetupEventArgs> Setup;
        public event EventHandler<VersionEventArgs> Version;

        protected override void HandleAddmeFailed()
        {
            AddmeFailed?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandleAddmeSuccess()
        {
            AddmeSuccess?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandleGoodbye()
        {
            Goodbye?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandleSetup(string SetupCommand, string SetupValue)
        {
            Setup?.Invoke(this, new SetupEventArgs()
            {
                SetupCommand = SetupCommand,
                SetupValue = SetupValue,
            });
        }

        protected override void HandleVersion(int csval, int scval, string verstring)
        {
            Version?.Invoke(this, new VersionEventArgs()
            {
                ClientToServerProtocolVersion = csval,
                ServerToClientProtocolVersion = scval,
                ClientVersionString = verstring.Trim(),
            });
        }

        public class SetupEventArgs : BatchEventArgs
        {
            public string SetupCommand { get; set; }
            public string SetupValue { get; set; }
        }

        public class VersionEventArgs : MessageHandlerEventArgs
        {
            public int ClientToServerProtocolVersion { get; set; }
            public int ServerToClientProtocolVersion { get; set; }
            public string ClientVersionString { get; set; }
        }
    }
}

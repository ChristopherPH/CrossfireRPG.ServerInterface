﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParser
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
                ClientVersionString = verstring,
            });
        }

        public class SetupEventArgs : MultiCommandEventArgs
        {
            public string SetupCommand { get; set; }
            public string SetupValue { get; set; }
        }

        public class VersionEventArgs : SingleCommandEventArgs
        {
            public int ClientToServerProtocolVersion { get; set; }
            public int ServerToClientProtocolVersion { get; set; }
            public string ClientVersionString { get; set; }
        }
    }
}

﻿/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using CrossfireRPG.ServerInterface.Definitions;
using System;

namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageHandler
    {
        public event EventHandler<DrawInfoEventArgs> DrawInfo;
        public event EventHandler<DrawExtInfoEventArgs> DrawExtInfo;
        public event EventHandler<FailureEventArgs> Failure;

        protected override void HandleDrawInfo(NewClient.NewDrawInfo Color,
            string Message)
        {
            DrawInfo?.Invoke(this, new DrawInfoEventArgs()
            {
                Color = Color,
                Message = Message
            });
        }

        protected override void HandleDrawExtInfo(NewClient.NewDrawInfo Flags,
            NewClient.MsgTypes MessageType, int SubType, string Message)
        {
            DrawExtInfo?.Invoke(this, new DrawExtInfoEventArgs()
            {
                Flags = Flags,
                MessageType = MessageType,
                SubType = SubType,
                Message = Message
            });
        }

        protected override void HandleFailure(string ProtocolCommand, string FailureString)
        {
            Failure?.Invoke(this, new FailureEventArgs()
            {
                ProtocolCommand = ProtocolCommand,
                FailureString = FailureString
            });
        }

        public class DrawInfoEventArgs : MessageHandlerEventArgs
        {
            public NewClient.NewDrawInfo Color { get; set; }
            public string Message { get; set; }
        }

        public class DrawExtInfoEventArgs : MessageHandlerEventArgs
        {
            public NewClient.NewDrawInfo Flags { get; set; }
            public NewClient.MsgTypes MessageType { get; set; }
            public int SubType { get; set; }
            public string Message { get; set; }
        }

        public class FailureEventArgs : MessageHandlerEventArgs
        {
            public string ProtocolCommand { get; set; }
            public string FailureString { get; set; }
        }
    }
}

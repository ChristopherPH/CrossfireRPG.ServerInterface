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
        public event EventHandler<ReplyInfoEventArgs> ReplyInfo;

        protected override void HandleReplyInfo(string request, byte[] reply)
        {
            ReplyInfo?.Invoke(this, new ReplyInfoEventArgs()
            {
                Request = request,
                Reply = reply
            });
        }

        public class ReplyInfoEventArgs : MessageHandlerEventArgs
        {
            public string Request { get; set; }
            public byte[] Reply { get; set; }
        }
    }
}

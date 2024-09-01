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
    /// <summary>
    /// Base event args for messages that raise a batch of events
    /// Valid BatchEventArgs will always appear between BeginBatch
    /// and EndBatch events
    /// </summary>
    public class BatchEventArgs : MessageHandlerEventArgs
    {
        /// <summary>
        /// 1 based event number between BeginBatch and EndBatch events
        /// </summary>
        public int BatchNumber { get; set; } = 0;

        /// <summary>
        /// Number of events expected between BeginBatch and EndBatch events
        /// </summary>
        public int BatchCount { get; set; } = 0;

        public bool IsFirstBatchEvent => BatchNumber == 1;
        public bool IsLastBatchEvent => BatchNumber == BatchCount;
        public bool OnlyOneMessage => IsFirstBatchEvent && IsLastBatchEvent;
    }

    public class BeginBatchEventArgs : EventArgs
    {
        /// <summary>
        /// Number of events expected between BeginBatch and EndBatch events
        /// </summary>
        public int BatchCount { get; set; }
    }

    public class EndBatchEventArgs : EventArgs
    {
        /// <summary>
        /// Number of events raised between BeginBatch and EndBatch events
        /// </summary>
        public int BatchCount { get; set; }
    }
}

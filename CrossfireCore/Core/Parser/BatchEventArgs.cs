using System;

namespace CrossfireCore.ServerInterface
{
    /// <summary>
    /// Base event args for messages that raise a batch of events
    /// Valid BatchEventArgs will always appear between BeginBatch
    /// and EndBatch events
    /// </summary>
    public class BatchEventArgs : MessageHandlerEventArgs
    {
        /// <summary>
        /// 0 based event number between BeginBatch and EndBatch events
        /// </summary>
        public int BatchNumber { get; set; } = 0;

        /// <summary>
        /// Number of events expected between BeginBatch and EndBatch events
        /// </summary>
        public int BatchCount { get; set; } = 0;

        public bool IsFirstBatchEvent => BatchNumber == 0;
        public bool IsLastBatchEvent => (BatchNumber + 1) == BatchCount;
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

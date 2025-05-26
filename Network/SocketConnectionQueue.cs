/*
 * Copyright (c) 2025 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System.Collections.Generic;
using System.Threading;

namespace CrossfireRPG.ServerInterface.Network
{
    /// <summary>
    /// Socket Connection Queue is a specialized SocketConnection that queues packets received from the server
    /// and processes them synchronously using a thread pool. This is useful for scenarios where packets may
    /// arrive faster than they can be processed.
    /// </summary>
    /* Note: This code is based on the MSDN article on ordered execution with ThreadPool and uses example code
     * from that article as a reference. See the article for more details and licensing information:
     * https://learn.microsoft.com/en-us/archive/msdn-magazine/2009/february/net-matters-ordered-execution-with-threadpool#Anchor_3
     */
    public class SocketConnectionQueue : SocketConnection
    {
        const int QueuedPacketWarningStep = 100;

        private Queue<byte[]> _queuedPackets = new Queue<byte[]>();
        private bool _processingPacket = false;
        private int _queuedPacketsWarn = QueuedPacketWarningStep;

        protected override void RaiseOnPacket(byte[] Packet)
        {
            lock (_queuedPackets)
            {
                // Enqueue the packet for processing
                _queuedPackets.Enqueue(Packet);

                // Log a warning if the queued packet count exceeds the threshold
                if (_queuedPackets.Count >= _queuedPacketsWarn)
                {
                    Logger.Warning($"Queued packet count reached {_queuedPackets.Count} packets");
                    _queuedPacketsWarn += QueuedPacketWarningStep; //increase warning threshold
                }

                // If not already processing packets, start processing them
                if (!_processingPacket)
                {
                    _processingPacket = true;

                    ThreadPool.UnsafeQueueUserWorkItem(ProcessPackets, null);
                }
            }
        }

        private void ProcessPackets(object o)
        {
            // This method processes packets in a loop until there are no more packets to process
            while (true)
            {
                byte[] packet;

                lock (_queuedPackets)
                {
                    // If there are no packets to process, exit the loop
                    if (_queuedPackets.Count == 0)
                    {
                        _processingPacket = false;
                        _queuedPacketsWarn = QueuedPacketWarningStep; //reset warning threshold
                        break;
                    }

                    // Dequeue the next packet to process
                    packet = _queuedPackets.Dequeue();
                }

                try
                {
                    // Process the packet using the base class method
                    base.RaiseOnPacket(packet);
                }
                catch
                {
                    // If an exception occurs, re-queue the processing method to try again
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessPackets, null);
                    throw;
                }
            }
        }
    }
}

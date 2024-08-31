using System;

namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageBuilder
    {
        /// <summary>
        /// Applies an object (Ready/Wield/Wear/Activate/Open/Close/Read/Eat/Use/...)
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>true if protocol message sent</returns>
        public bool SendApply(Int32 tag)
        {
            using (var ba = new BufferAssembler("apply"))
            {
                //Apply expects a signed Int32
                //Note that due to pseudo objects having the high bit of an 32bit int set,
                //on an x86 server a negative value must be sent for the server to be able
                //to parse said high bit.
                ba.AddIntAsString(tag);

                return SendMessage(ba);
            }
        }

        /// <summary>
        /// Examines an object
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>true if protocol message sent</returns>
        public bool SendExamine(Int32 tag)
        {
            using (var ba = new BufferAssembler("examine"))
            {
                //Examine expects a signed Int32
                //Note that due to pseudo objects having the high bit of an 32bit int set,
                //on an x86 server a negative value must be sent for the server to be able
                //to parse said high bit.
                ba.AddIntAsString(tag);

                return SendMessage(ba);
            }
        }

        /// <summary>
        /// Moves item(s)
        /// </summary>
        /// <param name="to">Destination item tag (container, player), 0=move to ground</param>
        /// <param name="tag">Object tag to move</param>
        /// <param name="nrof">Number of items to move, 0=all</param>
        /// <returns>true if protocol message sent</returns>
        public bool SendMove(Int32 to, Int32 tag, Int32 nrof = 0)
        {
            using (var ba = new BufferAssembler("move"))
            {
                ba.AddIntAsString(to);
                ba.AddSpace();
                ba.AddIntAsString(tag);
                ba.AddSpace();
                ba.AddIntAsString(nrof);

                return SendMessage(ba);
            }
        }

        /// <summary>
        /// Locks an item in the players inventory
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>true if protocol message sent</returns>
        public bool SendLock(UInt32 tag)
        {
            using (var ba = new BufferAssembler("lock"))
            {
                ba.AddByte((byte)1);
                ba.AddUInt32(tag);

                return SendMessage(ba);
            }
        }

        /// <summary>
        /// Unlocks an item in the players inventory
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>true if protocol message sent</returns>
        public bool SendUnlock(UInt32 tag)
        {
            using (var ba = new BufferAssembler("lock"))
            {
                ba.AddByte((byte)0);
                ba.AddUInt32(tag);

                return SendMessage(ba);
            }
        }

        /// <summary>
        /// Marks an item in the players inventory
        /// </summary>
        /// <param name="tag"></param>
        /// <returns>true if protocol message sent</returns>
        public bool SendMark(UInt32 tag)
        {
            using (var ba = new BufferAssembler("mark"))
            {
                ba.AddUInt32(tag);

                return SendMessage(ba);
            }
        }
    }
}

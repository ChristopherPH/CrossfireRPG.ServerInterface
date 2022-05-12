using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageBuilder
    {
        /// <summary>
        /// Applies an object (Ready/Wield/Wear/Activate/Open/Close/Read/Eat/Use/...)
        /// </summary>
        /// <param name="tag"></param>
        public bool SendApply(string tag)
        {
            using (var ba = new BufferAssembler("apply"))
            {
                ba.AddString(tag);

                return SendMessage(ba);
            }
        }

        /// <summary>
        /// Always apply an object and never unapply it when already applied.
        /// </summary>
        public bool SendApplyOnly(string tag)
        {
            using (var ba = new BufferAssembler("apply"))
            {
                ba.AddString("-a");
                ba.AddSpace();
                ba.AddString(tag);

                return SendMessage(ba);
            }
        }

        /// <summary>
        /// Always unapply an object and never apply it when already unapplied.
        /// </summary>
        public bool SendApplyUnapply(string tag)
        {
            using (var ba = new BufferAssembler("apply"))
            {
                ba.AddString("-u");
                ba.AddSpace();
                ba.AddString(tag);

                return SendMessage(ba);
            }
        }

        /// <summary>
        /// Always open a container regardless of previous state.
        /// </summary>
        public bool SendApplyOpen(string tag)
        {
            using (var ba = new BufferAssembler("apply"))
            {
                ba.AddString("-o");
                ba.AddSpace();
                ba.AddString(tag);

                return SendMessage(ba);
            }
        }

        /// <summary>
        /// Apply an item on the ground or in the active container, but not one in the characters main inventory.
        /// </summary>
        public bool SendApplyGroundContainer(string tag)
        {
            using (var ba = new BufferAssembler("apply"))
            {
                ba.AddString("-b");
                ba.AddSpace();
                ba.AddString(tag);

                return SendMessage(ba);
            }
        }

        public bool SendExamine(string tag)
        {
            using (var ba = new BufferAssembler("examine"))
            {
                ba.AddString(tag);

                return SendMessage(ba);
            }
        }

        public bool SendMove(string to, string tag, string nrof = "0")
        {
            using (var ba = new BufferAssembler("move"))
            {
                ba.AddString("{0} {1} {2}", to, tag, nrof);

                return SendMessage(ba);
            }
        }

        public bool SendLock(UInt32 tag)
        {
            using (var ba = new BufferAssembler("lock"))
            {
                ba.AddByte((byte)1);
                ba.AddUInt32(tag);

                return SendMessage(ba);
            }
        }

        public bool SendUnlock(UInt32 tag)
        {
            using (var ba = new BufferAssembler("lock"))
            {
                ba.AddByte((byte)0);
                ba.AddUInt32(tag);

                return SendMessage(ba);
            }
        }

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

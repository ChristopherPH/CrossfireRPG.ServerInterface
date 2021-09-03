﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossfireCore.ServerInterface
{
    public class MessageBuilder
    {
        public const int ClientProtocolVersion = 1023;

        public MessageBuilder(SocketConnection Connection)
        {
            this.Connection = Connection;
        }

        private SocketConnection Connection;
        private UInt16 nComPacket = 1;

        public UInt16 SendNewCommand(string command, UInt32 repeat = 1)
        {
            using (var cb = new BufferAssembler("ncom ")) //NewCommand
            {
                var rc = nComPacket;
                cb.AddUInt16(rc);
                cb.AddUInt32(repeat);
                cb.AddString(command);

                nComPacket++;
                if (nComPacket == 0)
                    nComPacket = 1;

                if (Connection.SendMessage(cb.GetBytes()) == false)
                    return 0;

                return rc;
            }
        }

        /// <summary>
        /// Start playing with the given PlayerName, assuming there is a logged in account
        /// </summary>
        /// <param name="PlayerName"></param>
        public void SendAccountPlay(string PlayerName)
        {
            Connection.SendMessage("accountplay {0}", PlayerName);
        }

        public void SendAccountLogin(string UserName, string Password)
        {
            using (var cb = new BufferAssembler("accountlogin "))
            {
                cb.AddLengthPrefixedString(UserName);
                cb.AddLengthPrefixedString(Password);

                Connection.SendMessage(cb.GetBytes());
            }
        }

        public void SendVersion(int ClientToServer, int ServerToClient, string ClientName)
        {
            Connection.SendMessage("version {0} {1} {2}", ClientToServer, ServerToClient, ClientName);
        }

        public void SendSetup(string SetupParameter, string SetupValue)
        {
            Connection.SendMessage("setup {0} {1}", SetupParameter, SetupValue);
        }

        public void SendRequestInfo(string Request)
        {
            Connection.SendMessage("requestinfo {0}", Request);
        }

        public void SendApply(string tag)
        {
            Connection.SendMessage("apply " + tag);
        }

        public void SendExamine(string tag)
        {
            Connection.SendMessage("examine " + tag);
        }

        public void SendMove(string to, string tag, string nrof = "0")
        {
            Connection.SendMessage(string.Format("move {0} {1} {2}",
                to, tag, nrof));
        }

        public void SendLock(UInt32 tag)
        {
            using (var cb = new BufferAssembler("lock "))
            {
                cb.AddByte((byte)1);
                cb.AddUInt32(tag);

                Connection.SendMessage(cb.GetBytes());
            }
        }

        public void SendUnlock(UInt32 tag)
        {
            using (var cb = new BufferAssembler("lock "))
            {
                cb.AddByte((byte)0 );
                cb.AddUInt32(tag);

                Connection.SendMessage(cb.GetBytes());
            }
        }

        public void SendMark(UInt32 tag)
        {
            using (var cb = new BufferAssembler("mark "))
            {
                cb.AddUInt32(tag);

                Connection.SendMessage(cb.GetBytes());
            }
        }

        public void SendAskFace(Int32 tag)
        {
            using (var cb = new BufferAssembler("askface "))
            {
                cb.AddInt32(tag);

                Connection.SendMessage(cb.GetBytes());
            }
        }
    }
}

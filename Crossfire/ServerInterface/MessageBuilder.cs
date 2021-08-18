using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.ServerInterface
{
    public class MessageBuilder
    {
        public const int ClientProtocolVersion = 1023;

        public MessageBuilder(SocketConnection Connection)
        {
            this.Connection = Connection;
        }

        private SocketConnection Connection;

        private void SendMessage(string Message)
        {
            Connection.SendMessage(Message);
        }

        private void SendMessage(string Format, params object[] args)
        {
            SendMessage(string.Format(Format, args));
        }

        private UInt16 nComPacket = 0;

        public UInt16 SendCommand(string command, UInt32 repeat = 1)
        {
            using (var cb = new BufferAssembler("ncom ")) //newcommand
            {
                var rc = nComPacket;
                cb.AddInt16(rc);
                cb.AddInt32(repeat);
                cb.AddString(command);

                System.Diagnostics.Debug.Print("Sending Command {0}:{1}", nComPacket, command);

                nComPacket++;
                Connection.SendMessage(cb.GetBytes());

                return rc;
            }
        }

        /// <summary>
        /// Start playing with the given PlayerName, assuming there is a logged in account
        /// </summary>
        /// <param name="PlayerName"></param>
        public void SendAccountPlay(string PlayerName)
        {
            SendMessage("accountplay {0}", PlayerName);
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
            SendMessage("version {0} {1} {2}", ClientToServer, ServerToClient, ClientName);
        }

        public void SendSetup(string SetupParameter, string SetupValue)
        {
            SendMessage("setup {0} {1}", SetupParameter, SetupValue);
        }

        public void SendRequestInfo(string Request)
        {
            SendMessage("requestinfo {0}", Request);
        }

        public void SendApply(string tag)
        {
            Connection.SendMessage("apply " + tag);
        }
    }
}

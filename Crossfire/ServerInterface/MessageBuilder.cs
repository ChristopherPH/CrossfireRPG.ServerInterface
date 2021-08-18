using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.ServerInterface
{
    public class MessageBuilder
    {
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

        public void SendSetup(string SetupParameter, string SetupValue)
        {
            SendMessage("setup {0} {1}", SetupParameter, SetupValue);
        }
    }
}

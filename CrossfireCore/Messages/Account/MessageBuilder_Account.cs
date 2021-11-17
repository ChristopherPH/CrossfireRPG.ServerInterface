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
        /// Start playing with the given PlayerName, assuming there is a logged in account
        /// </summary>
        /// <param name="PlayerName"></param>
        public void SendAccountPlay(string PlayerName)
        {
            using (var ba = new BufferAssembler("accountplay"))
            {
                ba.AddString(PlayerName);

                SendMessage(ba);
            }
        }

        public void SendAccountLogin(string UserName, string Password)
        {
            using (var ba = new BufferAssembler("accountlogin"))
            {
                ba.AddLengthPrefixedString(UserName);
                ba.AddLengthPrefixedString(Password);

                SendMessage(ba);
            }
        }

        public void SendAccountNew(string UserName, string Password)
        {
            using (var ba = new BufferAssembler("accountnew"))
            {
                ba.AddLengthPrefixedString(UserName);
                ba.AddLengthPrefixedString(Password);

                SendMessage(ba);
            }
        }

        public void SendAccountAddPlayer(bool Force, string UserName, string Password)
        {
            using (var ba = new BufferAssembler("accountaddplayer"))
            {
                ba.AddByte(Force ? (byte)1 : (byte)0);
                ba.AddLengthPrefixedString(UserName);
                ba.AddLengthPrefixedString(Password);

                SendMessage(ba);
            }
        }

        public void SendAccountPW(string OldPassword, string NewPassword)
        {
            using (var ba = new BufferAssembler("accountpw"))
            {
                ba.AddLengthPrefixedString(OldPassword);
                ba.AddLengthPrefixedString(NewPassword);

                SendMessage(ba);
            }
        }
    }
}

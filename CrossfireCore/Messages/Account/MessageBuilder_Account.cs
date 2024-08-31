namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageBuilder
    {
        /// <summary>
        /// Start playing with the given PlayerName, assuming there is a logged in account
        /// </summary>
        /// <param name="PlayerName"></param>
        public bool SendAccountPlay(string PlayerName)
        {
            using (var ba = new BufferAssembler("accountplay"))
            {
                ba.AddString(PlayerName);

                return SendMessage(ba);
            }
        }

        public bool SendAccountLogin(string UserName, string Password)
        {
            using (var ba = new BufferAssembler("accountlogin"))
            {
                ba.AddLengthPrefixedString(UserName);
                ba.AddLengthPrefixedString(Password);

                return SendMessage(ba);
            }
        }

        public bool SendAccountNew(string UserName, string Password)
        {
            using (var ba = new BufferAssembler("accountnew"))
            {
                ba.AddLengthPrefixedString(UserName);
                ba.AddLengthPrefixedString(Password);

                return SendMessage(ba);
            }
        }

        public bool SendAccountAddPlayer(bool Force, string UserName, string Password)
        {
            using (var ba = new BufferAssembler("accountaddplayer"))
            {
                ba.AddByte(Force ? (byte)1 : (byte)0);
                ba.AddLengthPrefixedString(UserName);
                ba.AddLengthPrefixedString(Password);

                return SendMessage(ba);
            }
        }

        public bool SendAccountPW(string OldPassword, string NewPassword)
        {
            using (var ba = new BufferAssembler("accountpw"))
            {
                ba.AddLengthPrefixedString(OldPassword);
                ba.AddLengthPrefixedString(NewPassword);

                return SendMessage(ba);
            }
        }

        public bool SendAddMe()
        {
            using (var ba = new BufferAssembler("addme", false))
            {
                return SendMessage(ba);
            }
        }
    }
}

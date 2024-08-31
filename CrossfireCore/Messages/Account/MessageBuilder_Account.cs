namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageBuilder
    {
        /// <summary>
        /// Start playing with the given PlayerName, assuming there is a logged in account
        /// </summary>
        /// <param name="PlayerName"></param>
        public bool SendProtocolAccountPlay(string PlayerName)
        {
            using (var ba = new BufferAssembler("accountplay"))
            {
                ba.AddString(PlayerName);

                return SendProtocolMessage(ba);
            }
        }

        public bool SendProtocolAccountLogin(string UserName, string Password)
        {
            using (var ba = new BufferAssembler("accountlogin"))
            {
                ba.AddLengthPrefixedString(UserName);
                ba.AddLengthPrefixedString(Password);

                return SendProtocolMessage(ba);
            }
        }

        public bool SendProtocolAccountNew(string UserName, string Password)
        {
            using (var ba = new BufferAssembler("accountnew"))
            {
                ba.AddLengthPrefixedString(UserName);
                ba.AddLengthPrefixedString(Password);

                return SendProtocolMessage(ba);
            }
        }

        public bool SendProtocolAccountAddPlayer(bool Force, string UserName, string Password)
        {
            using (var ba = new BufferAssembler("accountaddplayer"))
            {
                ba.AddByte(Force ? (byte)1 : (byte)0);
                ba.AddLengthPrefixedString(UserName);
                ba.AddLengthPrefixedString(Password);

                return SendProtocolMessage(ba);
            }
        }

        public bool SendProtocolAccountPW(string OldPassword, string NewPassword)
        {
            using (var ba = new BufferAssembler("accountpw"))
            {
                ba.AddLengthPrefixedString(OldPassword);
                ba.AddLengthPrefixedString(NewPassword);

                return SendProtocolMessage(ba);
            }
        }

        public bool SendProtocolAddMe()
        {
            using (var ba = new BufferAssembler("addme", false))
            {
                return SendProtocolMessage(ba);
            }
        }
    }
}

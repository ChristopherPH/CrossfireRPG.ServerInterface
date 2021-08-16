using Crossfire.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire
{
    public static class Protocol
    {
        
        public static Parser parser { get; } = new Parser();

        public static Connection Connection
        {
            get => _Connection;
            set
            {
                if (_Connection == value)
                    return;

                if (_Connection != null)
                {
                    _Connection.OnPacket -= parser.ParsePacket;
                    parser.Version -= Parser_Version;
                    parser.AccountPlayer -= Parser_AccountPlayer;
                }

                _Connection = value;

                if (_Connection != null)
                {
                    _Connection.OnPacket += parser.ParsePacket;

                    parser.Version += Parser_Version;
                    parser.AccountPlayer += Parser_AccountPlayer;
                }
            }
        }

        private static Connection _Connection;

        const int MinimumClientToServerVersion = 1023;
        const int MinimumServerToClientVersion = 1039;

        private static void Parser_Version(object sender, Parser.VersionEventArgs e)
        {
            if (e.ClientToServerProtocolVersion < MinimumClientToServerVersion)
                return;
            if (e.ServerToClientProtocolVersion > MinimumServerToClientVersion)
                return;

            SendAddClient();
        }

        private static void Parser_AccountPlayer(object sender, Parser.AccountPlayerEventArgs e)
        {
            SendAccountPlay(e.PlayerName);
        }

 

        public static void SendAddClient()
        {
            _Connection.SendMessage("version 1023 1029 Cloak's Windows Native Client");
            //_Connection.SendMessage("addme");
        }

        public static void SendAccountLogin(string UserName, string Password)
        {
            using (var cb = new CommandBuilder("accountlogin "))
            {
                cb.AddLengthPrefixedString(UserName);
                cb.AddLengthPrefixedString(Password);

                _Connection.SendMessage(cb.GetBytes());
            }
        }

        public static void SendAccountPlay(string PlayerName)
        {
            _Connection.SendMessage("accountplay " + PlayerName);
        }

        public static void SendApply()
        {
            _Connection.SendMessage("apply");
        }
    }
}

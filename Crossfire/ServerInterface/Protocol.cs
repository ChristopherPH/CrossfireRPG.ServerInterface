using Crossfire.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.ServerInterface
{
    public static class Protocol
    {

        const int MinimumClientToServerVersion = 1023;

        public static int LoginMethod { get; set; } = 1;

        
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
                }

                _Connection = value;

                if (_Connection != null)
                {
                    _Connection.OnPacket += parser.ParsePacket;

                    parser.Version += Parser_Version;
                    parser.Setup += Parser_Setup;
                }
            }
        }

        private static void Parser_Setup(object sender, Parser.SetupEventArgs e)
        {
            switch (e.SetupCommand.ToLower())
            {
                case "loginmethod": 
                    var newLoginMethod = int.Parse(e.SetupValue);
                    if (newLoginMethod != LoginMethod)
                    {
                        LoginMethod = newLoginMethod;
                    }

                    //Send command when not using the account based login
                    if (LoginMethod == 0)
                        _Connection.SendMessage("addme");
                    else
                    {
                        _Connection.SendMessage("requestinfo motd");
                        _Connection.SendMessage("requestinfo news");
                        _Connection.SendMessage("requestinfo rules");
                    }
                    break;
            }
        }

        private static Connection _Connection;


        private static void Parser_Version(object sender, Parser.VersionEventArgs e)
        {
            //TODO: validate this
            if (e.ClientToServerProtocolVersion < MinimumClientToServerVersion)
                return;
            if (e.ServerToClientProtocolVersion > CommandParser.ServerProtocolVersion)
                return;

            SetupClientConnection();
        }

        public static void SetupClientConnection()
        {
            _Connection.SendMessage("version 1023 1029 Cloak's Windows Native Client");

            _Connection.SendMessage("requestinfo skill_info");
            _Connection.SendMessage("requestinfo exp_table");

            _Connection.SendMessage("setup loginmethod " + LoginMethod.ToString());
        }

        public static void SendAccountLogin(string UserName, string Password)
        {
            using (var cb = new CommandBuilder("accountlogin "))
            {
                cb.AddLengthPrefixedString(UserName);
                cb.AddLengthPrefixedString(Password);

                _Connection.SendMessage(cb.GetBytes());
                //cb.SendMessage(_Connection);
            }
        }

        /// <summary>
        /// Start playing with the given PlayerName, assuming there is a logged in account
        /// </summary>
        /// <param name="PlayerName"></param>
        public static void SendAccountPlay(string PlayerName)
        {
            _Connection.SendMessage("accountplay " + PlayerName);
        }

        public static void SendApply(string tag)
        {
            _Connection.SendMessage("apply " + tag);
        }

        private static UInt16 nComPacket = 0;

        public static void SendCommand(string command, UInt32 repeat = 1)
        {
            using (var cb = new CommandBuilder("ncom ")) //newcommand
            {
                cb.AddInt16(nComPacket++);
                cb.AddInt32(repeat);
                cb.AddString(command);

                _Connection.SendMessage(cb.GetBytes());
            }
        }
    }
}

using Crossfire.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.ServerInterface
{
    public class Protocol
    {

        const int MinimumClientToServerVersion = 1023;

        public static int LoginMethod { get; set; } = 1;

        
        public Parser parser { get; } = new Parser();

        public Connection Connection
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
                    parser.Image2 += Parser_Image2;
                }
            }
        }

        public Dictionary<uint, Image> Faces { get; } = new Dictionary<uint, Image>();

        private void Parser_Image2(object sender, Parser.Image2EventArgs e)
        {
            using (var ms = new MemoryStream(e.ImageData))
            {
                Image image = Image.FromStream(ms);

                System.Diagnostics.Debug.Assert(image != null);

                Faces[e.ImageFace] = image;
            }
        }

        private void Parser_Setup(object sender, Parser.SetupEventArgs e)
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

        private Connection _Connection;


        private void Parser_Version(object sender, Parser.VersionEventArgs e)
        {
            //TODO: validate this
            if (e.ClientToServerProtocolVersion < MinimumClientToServerVersion)
                return;
            if (e.ServerToClientProtocolVersion > CommandParser.ServerProtocolVersion)
                return;

            SetupClientConnection();
        }

        public void SetupClientConnection()
        {
            _Connection.SendMessage("version 1023 1029 Cloak's Windows Native Client");

            _Connection.SendMessage("requestinfo skill_info");
            _Connection.SendMessage("requestinfo exp_table");

            _Connection.SendMessage("setup loginmethod " + LoginMethod.ToString());
        }

        public void SendAccountLogin(string UserName, string Password)
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
        public void SendAccountPlay(string PlayerName)
        {
            _Connection.SendMessage("accountplay " + PlayerName);
        }

        public void SendApply(string tag)
        {
            _Connection.SendMessage("apply " + tag);
        }

        private UInt16 nComPacket = 0;

        public void SendCommand(string command, UInt32 repeat = 1)
        {
            using (var cb = new CommandBuilder("ncom ")) //newcommand
            {
                cb.AddInt16(nComPacket);
                cb.AddInt32(repeat);
                cb.AddString(command);

                System.Diagnostics.Debug.Print("Sending Command {0}:{1}", nComPacket, command);

                nComPacket++;
                _Connection.SendMessage(cb.GetBytes());
            }
        }
    }
}

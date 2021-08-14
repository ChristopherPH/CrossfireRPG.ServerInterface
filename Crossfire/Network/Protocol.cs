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
                    _Connection.OnStatusChanged -= ConnectionStatusChanged;
                }

                _Connection = value;

                if (_Connection != null)
                {
                    _Connection.OnPacket += parser.ParsePacket;
                    _Connection.OnStatusChanged += ConnectionStatusChanged;

                    //if (_Connection.ConnectionStatus == ConnectionStatuses.Connected)
                    //    AddClient();

                    parser.Version += Parser_Version;

                }
            }
        }

        private static void Parser_Version(object sender, Parser.VersionEventArgs e)
        {
            _Connection.SendMessage("version 1023 1029 Chris Client");
            _Connection.SendMessage("addme");
        }

        private static Connection _Connection;

        public static Parser parser { get; } = new Parser();

        private static void ConnectionStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            System.Diagnostics.Debug.Assert(e != null);

            switch (e.Status)
            {
                case ConnectionStatuses.Disconnected:
                    break;

                case ConnectionStatuses.Connecting:
                    break;

                case ConnectionStatuses.Connected:
                    //AddClient();
                    break;
            }
        }

        private static void AddClient()
        {
            _Connection.SendMessage("version 1023 1029 Chris Client");
            _Connection.SendMessage("addme");
            //_Connection.SendMessage("bad command");
        }

        private static void Login()
        {

        }       
    }
}

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
                    _Connection.OnPacket -= ParsePacket;
                    _Connection.OnStatusChanged -= ConnectionStatusChanged;
                }

                _Connection = value;

                if (_Connection != null)
                {
                    _Connection.OnPacket += ParsePacket;
                    _Connection.OnStatusChanged += ConnectionStatusChanged;

                    if (_Connection.ConnectionStatus == ConnectionStatuses.Connected)
                        AddClient();
                }
            }
        }
        private static Connection _Connection;



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
                    AddClient();
                    break;
            }
        }

        private static void AddClient()
        {
            _Connection.SendMessage("version 1023 1029 Chris Client");
            //_Connection.SendMessage("addme");
        }

        private static void Login()
        {

        }

        private static void ParsePacket(object sender, ConnectionPacketEventArgs e)
        {
            System.Diagnostics.Debug.Assert(e != null);
            System.Diagnostics.Debug.Assert(e.Packet != null);
            System.Diagnostics.Debug.Assert(e.Packet.Length > 0);

            int offset = 0;
            var cmd = Tokenizer.GetString(e.Packet, ref offset);

            if (offset < e.Packet.Length)
            {
                Logger.Log(Logger.Levels.Debug, "S->C: cmd={0}, datalen={1}", cmd, e.Packet.Length - offset);
                Logger.Log(Logger.Levels.Debug, "{0}", HexDump.Utils.HexDump(e.Packet, 32));
            }
            else
                Logger.Log(Logger.Levels.Debug, "S->C: cmd={0}", cmd);

            switch (cmd)
            {
                case "version":
                    var csval = Tokenizer.GetStringInt(e.Packet, ref offset);
                    var scval = Tokenizer.GetStringInt(e.Packet, ref offset);
                    var verstr = Encoding.ASCII.GetString(e.Packet, offset, e.Packet.Length - offset);
                    break;

                case "failure":
                    var protocol_command = Tokenizer.GetString(e.Packet, ref offset);
                    var failure_string = Tokenizer.GetString(e.Packet, ref offset);
                    Logger.Log(Logger.Levels.Error, "Failure: {0} {1}", protocol_command, failure_string);
                    break;

                case "addme_success":
                    break;

                case "addme_failed":
                    break;

                case "goodbye":
                    break;

                default:
                    Logger.Log(Logger.Levels.Warn, "Unhandled Command: {0}", cmd);
                    break;
            }
        }
    }
}

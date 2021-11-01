using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossfireCore.ServerInterface
{
    public class MessageBuilder
    {
        static Logger _Logger = new Logger(nameof(MessageBuilder));

        public const int ClientProtocolVersion = 1023;

        public MessageBuilder(SocketConnection Connection)
        {
            this._Connection = Connection;
        }

        private SocketConnection _Connection;
        private UInt16 nComPacket = 1;

        private bool SendMessage(BufferAssembler ba)
        {
            _Logger.Info("C->S: cmd={0}, datalen={1}", ba.Command, ba.DataLength);
            return _Connection.SendMessage(ba.GetBytes());
        }

        public UInt16 SendNewCommand(string command, UInt32 repeat = 0)
        {
            using (var ba = new BufferAssembler("ncom")) //NewCommand
            {
                var tmpPacket = nComPacket;

                ba.AddUInt16(tmpPacket);
                ba.AddUInt32(repeat);
                ba.AddString(command);

                nComPacket++;
                if (nComPacket == 0)    //don't allow a commandNumber of 0
                    nComPacket = 1;

                if (SendMessage(ba) == false)
                    return 0;   //no command

                return tmpPacket;
            }
        }

        /// <summary>
        /// Start playing with the given PlayerName, assuming there is a logged in account
        /// </summary>
        /// <param name="PlayerName"></param>
        public void SendAccountPlay(string PlayerName)
        {
            using (var ba = new  BufferAssembler("accountplay"))
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

        public void SendCreatePlayer(string UserName, string Password)
        {
            using (var ba = new BufferAssembler("createplayer"))
            {
                ba.AddLengthPrefixedString(UserName);
                ba.AddLengthPrefixedString(Password);

                SendMessage(ba);
            }
        }

        public void SendCreatePlayer(string UserName, string Password, 
            string RaceArch, string ClassArch, IEnumerable<KeyValuePair<string, int>> Stats, 
            string StartingMapArch, IEnumerable<KeyValuePair<string, string>> Choices)
        {
            using (var ba = new BufferAssembler("createplayer"))
            {
                _Logger.Debug("Create Player: {0}", UserName);
                ba.AddLengthPrefixedString(UserName);

                //_Logger.Debug("Create Player: {0}", Password);
                ba.AddLengthPrefixedString(Password);

                if (!string.IsNullOrWhiteSpace(RaceArch))
                {
                    _Logger.Debug("Create Player: {0}", RaceArch);
                    ba.AddLengthPrefixedString("race {0}\0", RaceArch);
                }

                if (!string.IsNullOrWhiteSpace(ClassArch))
                {
                    _Logger.Debug("Create Player: {0}", ClassArch);
                    ba.AddLengthPrefixedString("class {0}\0", ClassArch);
                }

                if (Stats != null)
                {
                    foreach (var stat in Stats)
                    {
                        if (!string.IsNullOrWhiteSpace(stat.Key))
                        {
                            _Logger.Debug("Create Player: {0} {1}", stat.Key, stat.Value);
                            ba.AddLengthPrefixedString("{0} {1}\0", 
                                stat.Key, stat.Value);
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(StartingMapArch))
                {
                    _Logger.Debug("Create Player: {0}", StartingMapArch);
                    ba.AddLengthPrefixedString("starting_map {0}\0", StartingMapArch);
                }

                if (Choices != null)
                {
                    foreach (var choice in Choices)
                    {
                        if (!string.IsNullOrWhiteSpace(choice.Key) &&
                            !string.IsNullOrWhiteSpace(choice.Value))
                        {
                            _Logger.Debug("Create Player: choice {0} {1}", choice.Key, choice.Value);
                            ba.AddLengthPrefixedString("choice {0} {1}\0",
                                choice.Key, choice.Value);
                        }
                    }
                }

                SendMessage(ba);
            }
        }

        public void SendVersion(int ClientToServer, int ServerToClient, string ClientName)
        {
            using (var ba = new BufferAssembler("version"))
            {
                ba.AddString("{0} {1} {2}", ClientToServer, ServerToClient, ClientName);

                SendMessage(ba);
            }
        }

        public void SendSetup(string SetupParameter, string SetupValue)
        {
            using (var ba = new BufferAssembler("setup"))
            {
                ba.AddString("{0} {1}", SetupParameter, SetupValue);

                SendMessage(ba);
            }
        }

        public void SendSetup(string SetupParameter, int SetupValue)
        {
            using (var ba = new BufferAssembler("setup"))
            {
                ba.AddString("{0} {1}", SetupParameter, SetupValue);

                SendMessage(ba);
            }
        }

        public void SendSetup(string SetupParameter, bool SetupValue)
        {
            using (var ba = new BufferAssembler("setup"))
            {
                ba.AddString("{0} {1}", SetupParameter, SetupValue ? 1 : 0);

                SendMessage(ba);
            }
        }

        public void SendRequestInfo(string Request)
        {
            using (var ba = new BufferAssembler("requestinfo"))
            {
                ba.AddString(Request);

                SendMessage(ba);
            }
        }

        public void SendApply(string tag)
        {
            using (var ba = new BufferAssembler("apply"))
            {
                ba.AddString(tag);

                SendMessage(ba);
            }
        }

        public void SendExamine(string tag)
        {
            using (var ba = new BufferAssembler("examine"))
            {
                ba.AddString(tag);

                SendMessage(ba);
            }
        }

        public void SendMove(string to, string tag, string nrof = "0")
        {
            using (var ba = new BufferAssembler("move"))
            {
                ba.AddString("{0} {1} {2}", to, tag, nrof);

                SendMessage(ba);
            }
        }

        public void SendLock(UInt32 tag)
        {
            using (var ba = new BufferAssembler("lock"))
            {
                ba.AddByte((byte)1);
                ba.AddUInt32(tag);

                SendMessage(ba);
            }
        }

        public void SendUnlock(UInt32 tag)
        {
            using (var ba = new BufferAssembler("lock"))
            {
                ba.AddByte((byte)0);
                ba.AddUInt32(tag);

                SendMessage(ba);
            }
        }

        public void SendMark(UInt32 tag)
        {
            using (var ba = new BufferAssembler("mark"))
            {
                ba.AddUInt32(tag);

                SendMessage(ba);
            }
        }

        public void SendAskFace(Int32 tag)
        {
            using (var ba = new BufferAssembler("askface"))
            {
                ba.AddIntAsString(tag);

                SendMessage(ba);
            }
        }

        public void SendAskSmooth(Int32 tag)
        {
            using (var ba = new BufferAssembler("asksmooth"))
            {
                ba.AddIntAsString(tag);

                SendMessage(ba);
            }
        }

        public void SendLookAt(int x, int y)
        {
            using (var ba = new BufferAssembler("lookat"))
            {
                ba.AddString("{0} {1}", x, y);

                SendMessage(ba);
            }
        }

        public void SendReply(string Reply)
        {
            using (var ba = new BufferAssembler("reply"))
            {
                ba.AddString(Reply);

                SendMessage(ba);
            }
        }
    }
}

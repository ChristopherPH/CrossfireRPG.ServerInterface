using Common;
using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.Managers
{
    public class UserCommandManager
    {
        public UserCommandManager(SocketConnection Connection, MessageBuilder Builder, MessageParser Parser)
        {
            _Connection = Connection;
            _Builder = Builder;
            _Parser = Parser;

            Parser.CompletedCommand += Parser_CompletedCommand;
        }

        private SocketConnection _Connection;
        private MessageBuilder _Builder;
        private MessageParser _Parser;
        private bool WaitingForResponse = false;
        private UInt16 LastCommandID = 0;
        static Logger _Logger = new Logger(nameof(UserCommandManager));

        private void Parser_CompletedCommand(object sender, MessageParser.CompletedCommandEventArgs e)
        {
            if (e.Packet == LastCommandID)
            {
                _Logger.Debug("Received Command ID {0}: {1}ms", e.Packet, e.Time);

                LastCommandID = 0;
                WaitingForResponse = false;
            }
        }

        public event EventHandler<UserCommandEventArgs> OnUserCommand;

        public void SendUserCommand(string Command, uint Repeat = 0)
        {
            var args = new UserCommandEventArgs()
            {
                Command = Command,
                Repeat = Repeat,
            };

            //Allow any handlers to either change or cancel the message
            OnUserCommand?.Invoke(this, args);

            if (args.Cancel)
                return;

            //check to make sure we are not already waiting for a command
            //before sending this command
            if (!WaitingForResponse)
            {
                LastCommandID = _Builder.SendNewCommand(args.Command, args.Repeat);
                if (LastCommandID != 0)
                {
                    _Logger.Debug("Sending Command ID {0}: {1}",
                        LastCommandID, args.Command);
                    WaitingForResponse = true;
                }
            }
        }
    }

    public class UserCommandEventArgs : System.ComponentModel.CancelEventArgs
    {
        public string Command { get; set; } = string.Empty;
        public uint Repeat { get; set; } = 0;
    }
}

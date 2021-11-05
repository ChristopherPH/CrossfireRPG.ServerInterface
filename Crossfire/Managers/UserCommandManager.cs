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
        private List<UInt16> _WaitingIDs = new List<UInt16>();
        static Logger _Logger = new Logger(nameof(UserCommandManager));

        public event EventHandler<UserCommandEventArgs> OnUserCommand;

        private void Parser_CompletedCommand(object sender, MessageParser.CompletedCommandEventArgs e)
        {
            var commandID = e.Packet;

            if (WaitingForCommand(commandID))
            {
                _WaitingIDs.Remove(commandID);
                _Logger.Debug("Received Command ID {0}: {1}ms", e.Packet, e.Time);
            }
            else
            {
                _Logger.Warning("Received unexpected Command ID {0}", commandID);
            }
        }

        public UInt16 SendUserCommand(string format, params object[] args)
        {
            return SendUserCommand(string.Format(format, args));
        }

        public UInt16 SendUserCommand(string Command)
        {
            if (string.IsNullOrWhiteSpace(Command))
                return 0;

            var CommandList = Command.Split(';');
            UInt16 commandID = 0;

            foreach (var cmd in CommandList)
            {
                if (string.IsNullOrWhiteSpace(cmd))
                    continue;

                var args = new UserCommandEventArgs()
                {
                    Command = cmd.Trim(),
                };

                //Allow any handlers to either change or cancel the message
                OnUserCommand?.Invoke(this, args);

                if (args.Cancel)
                    continue;

                //ensure if command changed, it is still valid
                if (string.IsNullOrWhiteSpace(args.Command))
                    continue;

                commandID = _Builder.SendNewCommand(args.Command.Trim(), args.Repeat);
                if (commandID == 0)
                    continue;

                _Logger.Debug("Sending Command ID {0}: {1}", commandID, args.Command);

                if (WaitingForCommand(commandID))
                {
                    _Logger.Warning("Missed response for Command ID {0}", commandID);
                }
                else
                {
                    _WaitingIDs.Add(commandID);
                }
            }

            //return the last command id
            return commandID;
        }

        public bool WaitingForCommand(UInt16 CommandID)
        {
            if (CommandID == 0)
                return false;

            return _WaitingIDs.Contains(CommandID);
        }

        public void SendReadySkill(string Skill)
        {
            SendUserCommand("ready_skill {0}", Skill);
        }

        public void SendUseSkill(string Skill)
        {
            SendUserCommand("use_skill {0}", Skill);
        }

        public void SendCastSpell(string Spell)
        {
            SendUserCommand("cast {0}", Spell);
        }

        public void SendCastSpell(UInt32 Spell)
        {
            SendUserCommand("cast {0}", Spell);
        }

        public void SendCastSpell(UInt32 Spell, string SpellArgs)
        {
            SendUserCommand("cast {0} {1}", Spell, SpellArgs);
        }

        public void SendInvokeSpell(string Spell)
        {
            SendUserCommand("invoke {0}", Spell);
        }

        public void SendInvokeSpell(UInt32 Spell)
        {
            SendUserCommand("invoke {0}", Spell);
        }

        public void SendInvokeSpell(UInt32 Spell, string SpellArgs)
        {
            SendUserCommand("invoke {0} {1}", Spell, SpellArgs);
        }

        public void SendPickup(UInt32 PickupFlags)
        {
            SendUserCommand("pickup {0}", PickupFlags);
        }

        public void SendDelete()
        {
            SendUserCommand("delete");
        }

        public void SendKnowledgeShow(UInt32 KnowledgeID)
        {
            SendUserCommand("knowledge show {0}", KnowledgeID);
        }

        public void SendKnowledgeSearch(string Text)
        {
            SendUserCommand("knowledge search {0}", Text);
        }
    }

    public class UserCommandEventArgs : System.ComponentModel.CancelEventArgs
    {
        public string Command { get; set; } = string.Empty;
        public uint Repeat { get; set; } = 0;
    }
}

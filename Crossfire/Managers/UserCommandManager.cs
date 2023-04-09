using Common;
using CrossfireCore.Managers;
using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crossfire.Managers
{
    public class UserCommandManager : Manager
    {
        public UserCommandManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Connection.OnStatusChanged += _Connection_OnStatusChanged;
            Handler.CompletedCommand += Handler_CompletedCommand;
        }

        private List<UInt16> _WaitingIDs = new List<UInt16>();
        private object _WaitLock = new object();
        static Logger _Logger = new Logger(nameof(UserCommandManager));
        private Dictionary<string, List<CommandHandlerInfo>> _UserCommands = new Dictionary<string, List<CommandHandlerInfo>>();
        public IEnumerable<string> UserCommands => _UserCommands.Keys.OrderBy(x => x);
        public IEnumerable<KeyValuePair<string, string>> UserCommandDescriptions => _UserCommands
            .Select(x => new KeyValuePair<string, string>(x.Key, x.Value[0].Description))
            .OrderBy(x => x.Key);

        public delegate void UserCommandHandler(string Command, string CommandParameters);

        public event EventHandler<UserCommandEventArgs> OnUserCommand;

        private void _Connection_OnStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            lock (_WaitLock)
                _WaitingIDs.Clear();
        }

        private void Handler_CompletedCommand(object sender, MessageHandler.CompletedCommandEventArgs e)
        {
            var commandID = e.Packet;

            if (WaitingForCommand(commandID))
            {
                lock (_WaitLock)
                    _WaitingIDs.Remove(commandID);

                _Logger.Debug("Received Command ID {0}: {1}ms", commandID, e.Time);
            }
            else
            {
                _Logger.Warning("Received unexpected Command ID {0}", commandID);
            }
        }

        public UInt16 SendUserCommand(string format, params object[] args)
        {
            string s;

            try
            {
                s = string.Format(format, args);
            }
            catch
            {
                return 0;
            }

            return SendUserCommand(s);
        }

        public UInt16 SendUserCommand(string Command)
        {
            if (string.IsNullOrWhiteSpace(Command))
                return 0;

            var CommandList = Command.Split(';');
            UInt16 commandID = 0;

            foreach (var singleCommand in CommandList)
            {
                if (string.IsNullOrWhiteSpace(singleCommand))
                    continue;

                //clean up command and parameters
                var splitCommand = singleCommand.Trim().Split(new char[] { ' ' }, 2);

                if ((splitCommand == null) || (splitCommand.Length == 0))
                    continue;

                var tmpCommand = splitCommand[0];
                var tmpParams = splitCommand.Length == 2 ? splitCommand[1] : string.Empty;

                //if the command exists as a user command, then allow the configured handler to execute
                if (_UserCommands.TryGetValue(tmpCommand, out var commandInfos))
                {
                    foreach (var commandInfo in commandInfos)
                        commandInfo.Handler(tmpCommand, tmpParams);
                    continue;
                }

                //Allow any handlers to either change or cancel the message to the server
                var args = new UserCommandEventArgs()
                {
                    Command = tmpCommand,
                    Params = tmpParams,
                };

                //Allow any handlers to either change or cancel the message
                OnUserCommand?.Invoke(this, args);

                if (args.Cancel)
                    continue;

                //ensure if command changed, it is still valid
                if (string.IsNullOrWhiteSpace(args.Command))
                    continue;

                //reassemble the command
                tmpCommand = args.Command.Trim();

                if (!string.IsNullOrWhiteSpace(args.Params))
                    tmpCommand += " " + args.Params.Trim();

                //lock from sending the command to adding the ID, as we can receieve
                //Parser_CompletedCommand before calling _WaitingIDs.Add()
                lock (_WaitLock)
                {
                    commandID = Builder.SendNewCommand(tmpCommand, args.Repeat);
                    if (commandID == 0)
                        continue;

                    _Logger.Debug("Sending Command ID {0}: {1} [{2}]", commandID, tmpCommand, args.Repeat);

                    if (WaitingForCommand(commandID))
                    {
                        _Logger.Warning("Missed previous response for Command ID {0}", commandID);
                    }
                    else
                    {
                        _WaitingIDs.Add(commandID);
                    }
                }
            }

            //return the last command id
            return commandID;
        }

        public bool WaitingForCommand(UInt16 CommandID)
        {
            if (CommandID == 0)
                return false;

            lock (_WaitLock)
                return _WaitingIDs.Contains(CommandID);
        }

        public bool RegisterUserCommand(string Command, UserCommandHandler Handler, string Description)
        {
            if (string.IsNullOrWhiteSpace(Command) || (Handler == null))
                return false;

            var command = Command.ToLower().Trim();

            if (!_UserCommands.ContainsKey(command))
                _UserCommands[command] = new List<CommandHandlerInfo>();

            _UserCommands[command].Add(new CommandHandlerInfo()
            {
                Handler = Handler,
                Description = Description,
            });

            return true;
        }

        public void UnregisterUserCommand(string Command)
        {
            if (string.IsNullOrWhiteSpace(Command))
                return;

            if (!_UserCommands.ContainsKey(Command.ToLower()))
                return;

            _UserCommands.Remove(Command);
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

        public void SendKnowledgeAttempt(UInt32 KnowledgeID)
        {
            SendUserCommand("knowledge attempt {0}", KnowledgeID);
        }

        public void SendKnowledgeSearch(string Text)
        {
            SendUserCommand("knowledge search {0}", Text);
        }

        private class CommandHandlerInfo
        {
            public UserCommandHandler Handler { get; set; }
            public string Description { get; set; }
        }
    }

    public class UserCommandEventArgs : System.ComponentModel.CancelEventArgs
    {
        public string Command { get; set; } = string.Empty;
        public string Params { get; set; } = string.Empty;
        public uint Repeat { get; set; } = 0;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public class MultiCommandEventArgs : SingleCommandEventArgs
    {
        [Flags]
        public enum CommandTypes
        {
            FirstCommand = 0x01,
            LastCommand = 0x02,
            SingleCommand = FirstCommand | LastCommand
        }

        public CommandTypes CommandType { get; set; }
        public bool IsFirst => CommandType.HasFlag(CommandTypes.FirstCommand);
        public bool IsLast => CommandType.HasFlag(CommandTypes.LastCommand);
        public bool IsSingle => CommandType.HasFlag(CommandTypes.SingleCommand);
    }
}

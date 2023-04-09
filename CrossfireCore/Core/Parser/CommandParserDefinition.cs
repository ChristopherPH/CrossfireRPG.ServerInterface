using Common;

namespace CrossfireCore.ServerInterface
{
    public class CommandParserDefinition
    {
        public delegate bool CommandParser(byte[] Message, ref int DataOffset, int DataEnd);

        public CommandParserDefinition() { }

        public CommandParserDefinition(CommandParser Parser)
        {
            this.Parser = Parser;
        }

        public CommandParserDefinition(CommandParser Parser, Logger.Levels Level)
        {
            this.Parser = Parser;
            this.Level = Level;
        }

        public CommandParser Parser { get; set; }
        public Logger.Levels Level { get; set; } = Logger.Levels.Info;
    }
}

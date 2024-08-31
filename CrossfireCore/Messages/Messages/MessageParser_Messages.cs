using CrossfireRPG.ServerInterface.Definitions;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParser
    {
        protected abstract void HandleDrawInfo(NewClient.NewDrawInfo Color, string Message);
        protected abstract void HandleDrawExtInfo(NewClient.NewDrawInfo Flags,
            NewClient.MsgTypes MessageType, int SubType, string Message);
        protected abstract void HandleFailure(string ProtocolCommand, string FailureString);

        private void AddMessageParsers()
        {
            AddCommandHandler("drawinfo", new CommandParserDefinition(Parse_drawinfo));
            AddCommandHandler("drawextinfo", new CommandParserDefinition(Parse_drawextinfo));
            AddCommandHandler("failure", new CommandParserDefinition(Parse_failure));
        }

        private bool Parse_drawinfo(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var color = (NewClient.NewDrawInfo)BufferTokenizer.GetStringAsInt(Message, ref DataOffset, DataEnd);
            var message = BufferTokenizer.GetRemainingBytesAsString(Message, ref DataOffset, DataEnd);

            HandleDrawInfo(color, message);

            return true;
        }

        private bool Parse_drawextinfo(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var flags = (NewClient.NewDrawInfo)BufferTokenizer.GetStringAsInt(Message, ref DataOffset, DataEnd);
            var message_type = (NewClient.MsgTypes)BufferTokenizer.GetStringAsInt(Message, ref DataOffset, DataEnd);
            var sub_type = BufferTokenizer.GetStringAsInt(Message, ref DataOffset, DataEnd);
            var message = BufferTokenizer.GetRemainingBytesAsString(Message, ref DataOffset, DataEnd);

            HandleDrawExtInfo(flags, message_type, sub_type, message);

            return true;
        }

        private bool Parse_failure(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var protocol_command = BufferTokenizer.GetString(Message, ref DataOffset, DataEnd);
            var failure_string = BufferTokenizer.GetRemainingBytesAsString(Message, ref DataOffset, DataEnd);

            HandleFailure(protocol_command, failure_string);
            Logger.Error("Failure: {0} {1}", protocol_command, failure_string);

            return true;
        }
    }
}

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParser
    {
        /// <summary>
        /// Minimum protocol version to use Setup command
        /// </summary>
        public const int ServerProtocolVersionSetupCommand = 1026;

        protected abstract void HandleAddmeFailed();
        protected abstract void HandleAddmeSuccess();
        protected abstract void HandleGoodbye();
        protected abstract void HandleSetup(string SetupCommand, string SetupValue);
        protected abstract void HandleVersion(int csval, int scval, string verstring);

        private void AddProtocolParsers()
        {
            AddCommandHandler("addme_failed", new CommandParserDefinition(Parse_addme_failed));
            AddCommandHandler("addme_success", new CommandParserDefinition(Parse_addme_success));
            AddCommandHandler("goodbye", new CommandParserDefinition(Parse_goodbye));
            AddCommandHandler("setup", new CommandParserDefinition(Parse_setup));
            AddCommandHandler("version", new CommandParserDefinition(Parse_version));
        }

        private bool Parse_addme_failed(byte[] Message, ref int DataOffset, int DataEnd)
        {
            HandleAddmeFailed();

            return true;
        }

        private bool Parse_addme_success(byte[] Message, ref int DataOffset, int DataEnd)
        {
            HandleAddmeSuccess();

            return true;
        }

        private bool Parse_goodbye(byte[] Message, ref int DataOffset, int DataEnd)
        {
            HandleGoodbye();

            return true;
        }

        private bool Parse_setup(byte[] Message, ref int DataOffset, int DataEnd)
        {
            while (DataOffset < DataEnd)
            {
                var setup_command = BufferTokenizer.GetString(Message, ref DataOffset, DataEnd);
                var setup_value = BufferTokenizer.GetString(Message, ref DataOffset, DataEnd);

                HandleSetup(setup_command, setup_value);

                //Save spellmon for parser protocol
                if (setup_command == "spellmon" && setup_value != "FALSE")
                {
                    ParserOption_SpellMon = int.Parse(setup_value);
                }
            }

            return true;
        }

        private bool Parse_version(byte[] packet, ref int offset, int end)
        {
            var version_csval = BufferTokenizer.GetStringAsInt(packet, ref offset, end);
            ServerProtocolVersion = BufferTokenizer.GetStringAsInt(packet, ref offset, end);
            var version_verstr = BufferTokenizer.GetRemainingBytesAsString(packet, ref offset, end);

            HandleVersion(version_csval, ServerProtocolVersion, version_verstr);

            return true;
        }
    }
}

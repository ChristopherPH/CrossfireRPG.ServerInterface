using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParserBase
    {
        protected abstract void HandleAddmeFailed();
        protected abstract void HandleAddmeSuccess();
        protected abstract void HandleFailure(string ProtocolCommand, string FailureString);
        protected abstract void HandleGoodbye();
        protected abstract void HandleSetup(string SetupCommand, string SetupValue);
        protected abstract void HandleVersion(int csval, int scval, string verstring);

        private void AddProtocolParsers()
        {
            AddCommandHandler("addme_failed", new ParseCommand(Parse_addme_failed));
            AddCommandHandler("addme_success", new ParseCommand(Parse_addme_success));
            AddCommandHandler("failure", new ParseCommand(Parse_failure));
            AddCommandHandler("goodbye", new ParseCommand(Parse_goodbye));
            AddCommandHandler("setup", new ParseCommand(Parse_setup));
            AddCommandHandler("version", new ParseCommand(Parse_version));
        }

        private bool Parse_addme_failed(byte[] packet, ref int offset, int end)
        {
            HandleAddmeFailed();

            return true;
        }

        private bool Parse_addme_success(byte[] packet, ref int offset, int end)
        {
            HandleAddmeSuccess();

            return true;
        }

        private bool Parse_failure(byte[] packet, ref int offset, int end)
        {
            var protocol_command = BufferTokenizer.GetString(packet, ref offset, end);
            var failure_string = BufferTokenizer.GetRemainingBytesAsString(packet, ref offset, end);

            HandleFailure(protocol_command, failure_string);
            _Logger.Error("Failure: {0} {1}", protocol_command, failure_string);

            return true;
        }

        private bool Parse_goodbye(byte[] packet, ref int offset, int end)
        {
            HandleGoodbye();

            return true;
        }

        private bool Parse_setup(byte[] packet, ref int offset, int end)
        {
            while (offset < end)
            {
                var setup_command = BufferTokenizer.GetString(packet, ref offset, end);
                var setup_value = BufferTokenizer.GetString(packet, ref offset, end);

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
            var version_scval = BufferTokenizer.GetStringAsInt(packet, ref offset, end);
            var version_verstr = BufferTokenizer.GetRemainingBytesAsString(packet, ref offset, end);

            HandleVersion(version_csval, version_scval, version_verstr);

            return true;
        }
    }
}

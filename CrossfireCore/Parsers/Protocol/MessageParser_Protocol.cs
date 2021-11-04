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
            AddCommandHandler("addme_failed", Parse_addme_failed);
            AddCommandHandler("addme_success", Parse_addme_success);
            AddCommandHandler("failure", Parse_failure);
            AddCommandHandler("goodbye", Parse_goodbye);
            AddCommandHandler("setup", Parse_setup);
            AddCommandHandler("version", Parse_version);
        }

        private bool Parse_addme_failed(byte[] packet, ref int offset)
        {
            HandleAddmeFailed();

            return true;
        }

        private bool Parse_addme_success(byte[] packet, ref int offset)
        {
            HandleAddmeSuccess();

            return true;
        }

        private bool Parse_failure(byte[] packet, ref int offset)
        {
            var protocol_command = BufferTokenizer.GetString(packet, ref offset);
            var failure_string = BufferTokenizer.GetRemainingBytesAsString(packet, ref offset);

            HandleFailure(protocol_command, failure_string);
            _Logger.Error("Failure: {0} {1}", protocol_command, failure_string);

            return true;
        }

        private bool Parse_goodbye(byte[] packet, ref int offset)
        {
            HandleGoodbye();

            return true;
        }

        private bool Parse_setup(byte[] packet, ref int offset)
        {
            while (offset < packet.Length)
            {
                var setup_command = BufferTokenizer.GetString(packet, ref offset);
                var setup_value = BufferTokenizer.GetString(packet, ref offset);

                HandleSetup(setup_command, setup_value);

                //Save spellmon for parser protocol
                if (setup_command == "spellmon" && setup_value != "FALSE")
                {
                    ParserOption_SpellMon = int.Parse(setup_value);
                }
            }

            return true;
        }

        private bool Parse_version(byte[] packet, ref int offset)
        {
            var version_csval = BufferTokenizer.GetStringAsInt(packet, ref offset);
            var version_scval = BufferTokenizer.GetStringAsInt(packet, ref offset);
            var version_verstr = BufferTokenizer.GetRemainingBytesAsString(packet, ref offset);

            HandleVersion(version_csval, version_scval, version_verstr);

            return true;
        }
    }
}

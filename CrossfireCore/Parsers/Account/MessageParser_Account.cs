using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParserBase
    {
        protected abstract void HandleAccountPlayer(int PlayerCount, int PlayerNumber,
            UInt16 Level, UInt16 FaceNumber, string Name, string Class, string Race,
            string Face, string Party, string Map);

        private void AddAccountParsers()
        {
            AddCommandHandler("accountplayers", Parse_accountplayers);
        }

        private bool Parse_accountplayers(byte[] packet, ref int offset)
        {
            var num_characters = BufferTokenizer.GetByte(packet, ref offset);
            var character_count = 1;

            //TODO: change this to a beginplayer / endplayer event
            HandleAccountPlayer(num_characters, 0, 0, 0, "", "", "", "", "", "");

            UInt16 account_player_level = 0;
            UInt16 account_player_facenum = 0;
            string account_player_name = "";
            string account_player_class = "";
            string account_player_race = "";
            string account_player_face = "";
            string account_player_party = "";
            string account_player_map = "";

            while (offset < packet.Length)
            {
                var char_data_len = BufferTokenizer.GetByte(packet, ref offset);
                if (char_data_len == 0)
                {
                    HandleAccountPlayer(num_characters, character_count,
                        account_player_level, account_player_facenum, account_player_name,
                        account_player_class, account_player_race, account_player_face,
                        account_player_party, account_player_map);

                    character_count++;

                    account_player_level = 0;
                    account_player_facenum = 0;
                    account_player_name = "";
                    account_player_class = "";
                    account_player_race = "";
                    account_player_face = "";
                    account_player_party = "";
                    account_player_map = "";
                    continue;
                }

                var char_data_type = (NewClient.AccountCharacterLoginTypes)BufferTokenizer.GetByte(packet, ref offset);
                char_data_len--;

                switch (char_data_type)
                {
                    case NewClient.AccountCharacterLoginTypes.Level:
                        account_player_level = BufferTokenizer.GetUInt16(packet, ref offset);
                        break;

                    case NewClient.AccountCharacterLoginTypes.FaceNum:
                        account_player_facenum = BufferTokenizer.GetUInt16(packet, ref offset);
                        break;

                    case NewClient.AccountCharacterLoginTypes.Name:
                        account_player_name = BufferTokenizer.GetBytesAsString(packet, ref offset, char_data_len);
                        break;

                    case NewClient.AccountCharacterLoginTypes.Class:
                        account_player_class = BufferTokenizer.GetBytesAsString(packet, ref offset, char_data_len);
                        break;

                    case NewClient.AccountCharacterLoginTypes.Race:
                        account_player_race = BufferTokenizer.GetBytesAsString(packet, ref offset, char_data_len);
                        break;

                    case NewClient.AccountCharacterLoginTypes.Face:
                        account_player_face = BufferTokenizer.GetBytesAsString(packet, ref offset, char_data_len);
                        break;

                    case NewClient.AccountCharacterLoginTypes.Party:
                        account_player_party = BufferTokenizer.GetBytesAsString(packet, ref offset, char_data_len);
                        break;

                    case NewClient.AccountCharacterLoginTypes.Map:
                        account_player_map = BufferTokenizer.GetBytesAsString(packet, ref offset, char_data_len);
                        break;

                    default:
                        BufferTokenizer.GetBytes(packet, ref offset, char_data_len);
                        break;
                }
            }

            return true;
        }
    }
}

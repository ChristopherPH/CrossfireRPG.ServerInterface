using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParserBase
    {
        protected abstract void HandleMusic(string Song);
        protected abstract void HandleSound2(byte RelativeX, byte RelativeY, byte Direction, 
            byte Volume, byte SoundType, string SubType, string Name);

        private void AddAudioParsers()
        {
            AddCommandHandler("music", Parse_music);
            AddCommandHandler("sound2", Parse_sound2);
        }

        private bool Parse_music(byte[] packet, ref int offset)
        {
            var song = BufferTokenizer.GetRemainingBytesAsString(packet, ref offset);

            HandleMusic(song);

            return true;
        }

        private bool Parse_sound2(byte[] packet, ref int offset)
        {
            var sound_x = BufferTokenizer.GetByte(packet, ref offset);
            var sound_y = BufferTokenizer.GetByte(packet, ref offset);
            var sound_dir = BufferTokenizer.GetByte(packet, ref offset);
            var sound_vol = BufferTokenizer.GetByte(packet, ref offset);
            var sound_type = BufferTokenizer.GetByte(packet, ref offset);

            var sound_action_len = BufferTokenizer.GetByte(packet, ref offset);
            var sound_action = BufferTokenizer.GetBytesAsString(packet, ref offset, sound_action_len);

            var sound_name_len = BufferTokenizer.GetByte(packet, ref offset);
            var sound_name = BufferTokenizer.GetBytesAsString(packet, ref offset, sound_name_len);

            HandleSound2(sound_x, sound_y, sound_dir, sound_vol, sound_type, sound_action, sound_name);

            return true;
        }
    }
}

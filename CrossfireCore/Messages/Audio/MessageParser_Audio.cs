namespace CrossfireCore.ServerInterface
{
    public partial class MessageParser
    {
        protected abstract void HandleMusic(string Song);
        protected abstract void HandleSound2(byte RelativeX, byte RelativeY, byte Direction, 
            byte Volume, byte SoundType, string SubType, string Name);

        private void AddAudioParsers()
        {
            AddCommandHandler("music", new CommandParserDefinition(Parse_music));
            AddCommandHandler("sound2", new CommandParserDefinition(Parse_sound2));
        }

        private bool Parse_music(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var song = BufferTokenizer.GetRemainingBytesAsString(Message, ref DataOffset, DataEnd);

            HandleMusic(song);

            return true;
        }

        private bool Parse_sound2(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var sound_x = BufferTokenizer.GetByte(Message, ref DataOffset);
            var sound_y = BufferTokenizer.GetByte(Message, ref DataOffset);
            var sound_dir = BufferTokenizer.GetByte(Message, ref DataOffset);
            var sound_vol = BufferTokenizer.GetByte(Message, ref DataOffset);
            var sound_type = BufferTokenizer.GetByte(Message, ref DataOffset);

            var sound_action_len = BufferTokenizer.GetByte(Message, ref DataOffset);
            var sound_action = BufferTokenizer.GetBytesAsString(Message, ref DataOffset, sound_action_len);

            var sound_name_len = BufferTokenizer.GetByte(Message, ref DataOffset);
            var sound_name = BufferTokenizer.GetBytesAsString(Message, ref DataOffset, sound_name_len);

            HandleSound2(sound_x, sound_y, sound_dir, sound_vol, sound_type, sound_action, sound_name);

            return true;
        }
    }
}

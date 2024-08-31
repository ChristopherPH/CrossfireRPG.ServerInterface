using System;

namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageHandler
    {
        public event EventHandler<MusicEventArgs> Music;
        public event EventHandler<SoundEventArgs> Sound;

        protected override void HandleMusic(string Song)
        {
            Music?.Invoke(this, new MusicEventArgs()
            {
                Song = Song
            });
        }

        protected override void HandleSound2(byte RelativeX, byte RelativeY, byte Direction,
            byte Volume, byte SoundType, string SubType, string Name)
        {
            Sound?.Invoke(this, new SoundEventArgs()
            {
                RelativeX = RelativeX,
                RelativeY = RelativeY,
                Direction = Direction,
                Volume = Volume,
                SoundType = SoundType,
                SubType = SubType,
                Name = Name,
            });
        }

        public class MusicEventArgs : MessageHandlerEventArgs
        {
            public string Song { get; set; }
        }

        public class SoundEventArgs : MessageHandlerEventArgs
        {
            /// <summary>
            /// Relative To Player
            /// </summary>
            public byte RelativeX { get; set; }

            /// <summary>
            /// Relative To Player
            /// </summary>
            public byte RelativeY { get; set; }

            /// <summary>
            /// Direction, 0-8
            /// </summary>
            public byte Direction { get; set; }

            /// <summary>
            /// 0-100
            /// </summary>
            public byte Volume { get; set; }

            /// <summary>
            /// Type of sound (used as a base folder name for sound file)
            /// </summary>
            public byte SoundType { get; set; }

            /// <summary>
            /// Name of sound
            /// </summary>
            public string SubType { get; set; }

            /// <summary>
            /// Source of sound (object or player race)
            /// </summary>
            public string Name { get; set; }
        }
    }
}

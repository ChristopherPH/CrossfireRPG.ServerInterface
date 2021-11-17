using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.Parser
{
    public partial class MessageParser
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

        public class MusicEventArgs : SingleCommandEventArgs
        {
            public string Song { get; set; }
        }

        public class SoundEventArgs : SingleCommandEventArgs
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
            public byte SoundType { get; set; }
            public string SubType { get; set; }
            public string Name { get; set; }
        }
    }
}

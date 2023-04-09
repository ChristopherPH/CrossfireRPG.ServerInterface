using System;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageHandler
    {
        public event EventHandler<AccountPlayerEventArgs> AccountPlayer;

        protected override void HandleAccountPlayer(int PlayerCount, int PlayerNumber, UInt16 Level,
            UInt16 FaceNumber, string Name, string Class, string Race, string Face, string Party, string Map)
        {
            AccountPlayer?.Invoke(this, new AccountPlayerEventArgs()
            {
                PlayerCount = PlayerCount,
                PlayerNumber = PlayerNumber,
                Level = Level,
                FaceNumber = FaceNumber,
                Name = Name,
                Class = Class,
                Race = Race,
                Face = Face,
                Party = Party,
                Map = Map
            });
        }

        public class AccountPlayerEventArgs : MultiCommandEventArgs
        {
            public int PlayerCount { get; set; }
            public int PlayerNumber { get; set; }
            public UInt16 Level { get; set; }
            public UInt16 FaceNumber { get; set; }
            public string Name { get; set; }
            public string Class { get; set; }
            public string Race { get; set; }
            public string Face { get; set; }
            public string Party { get; set; }
            public string Map { get; set; }
        }
    }
}

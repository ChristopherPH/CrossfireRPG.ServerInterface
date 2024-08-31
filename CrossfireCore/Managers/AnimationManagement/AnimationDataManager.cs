using Common;
using CrossfireCore.ServerInterface;
using CrossfireRPG.ServerInterface.Network;
using System;
using System.Collections.Generic;

namespace CrossfireCore.Managers.AnimationManagement
{
    public class AnimationDataManager : DataManager
    {
        public AnimationDataManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Handler.Animation += _Handler_Animation;
        }

        public static Logger Logger { get; } = new Logger(nameof(AnimationDataManager));

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => false;

        protected override void ClearData(bool disconnected)
        {
            lock (_AnimationLock)
            {
                _Animations.Clear();
            }
        }

        private object _AnimationLock = new object();

        private Dictionary<UInt32, AnimationInfo> _Animations { get; } =
            new Dictionary<UInt32, AnimationInfo>();

        public event EventHandler<AnimationAvailableEventArgs> AnimationAvailable;

        public UInt16[] GetAnimationFrames(UInt16 AnimationNumber)
        {
            AnimationInfo animationInfo;
            bool hasAnimation;

            lock (_AnimationLock)
            {
                hasAnimation = _Animations.TryGetValue(AnimationNumber, out animationInfo);
            }

            if (!hasAnimation)
                return null;

            return animationInfo.Faces;
        }

        public UInt16 GetAnimationFrame(UInt16 AnimationNumber, int FrameNumber = 0)
        {
            AnimationInfo animationInfo;
            bool hasAnimation;

            lock (_AnimationLock)
            {
                hasAnimation = _Animations.TryGetValue(AnimationNumber, out animationInfo);
            }

            //Note: There is currently no way to request missing animations like there
            //      is faces, so animations won't be available unless the anim command
            //      has been previously sent via an object/map command.
            if (!hasAnimation)
                return 0;

            return animationInfo.GetFrame(FrameNumber);
        }

        public int GetAnimationFrameCount(UInt16 AnimationNumber)
        {
            AnimationInfo animationInfo;
            bool hasAnimation;

            lock (_AnimationLock)
            {
                hasAnimation = _Animations.TryGetValue(AnimationNumber, out animationInfo);
            }

            //Note: There is currently no way to request missing animations like there
            //      is faces, so animations won't be available unless the anim command
            //      has been previously sent via an object/map command.
            if (!hasAnimation)
                return 0;

            return animationInfo.FrameCount;
        }

        private void _Handler_Animation(object sender, MessageHandler.AnimationEventArgs e)
        {
            Logger.Info("Received Animation {0}", e.AnimationNumber);

            lock (_AnimationLock)
            {
                if (_Animations.ContainsKey(e.AnimationNumber))
                {
                    Logger.Error("Animation already exists " + e.AnimationNumber);
                }

                _Animations[e.AnimationNumber] = new AnimationInfo()
                {
                    Flags = e.AnimationFlags,
                    Faces = e.AnimationFaces,
                };
            }

            AnimationAvailable?.Invoke(this, new AnimationAvailableEventArgs()
            {
                AnimationNumber = e.AnimationNumber
            });
        }

        private class AnimationInfo
        {
            /// <summary>
            /// Currently unused
            /// </summary>
            public UInt16 Flags { get; set; }

            /// <summary>
            /// Array of faces, represent animation frames
            /// </summary>
            public UInt16[] Faces { get; set; }

            /// <summary>
            /// Number of faces/frames in animation
            /// </summary>
            public int FrameCount => Faces?.Length ?? 0;

            /// <summary>
            /// Gets a faces/frame given a frame number
            /// </summary>
            public UInt16 GetFrame(int FrameNumber)
            {
                if ((FrameNumber < 0) || (FrameNumber >= FrameCount))
                    return 0;

                return Faces[FrameNumber];
            }
        }
    }

    public class AnimationAvailableEventArgs : EventArgs
    {
        public UInt16 AnimationNumber { get; set; }
    }
}

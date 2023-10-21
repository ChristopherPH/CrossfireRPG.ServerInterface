﻿using Common;
using CrossfireCore.Managers;
using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;

namespace Crossfire.Managers
{
    public class AnimationDataManager : DataManager
    {
        public AnimationDataManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Handler.Animation += _Handler_Animation;
        }

        static Logger _Logger = new Logger(nameof(AnimationDataManager));

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => false;

        protected override void ClearData()
        {
            lock (_AnimationLock)
            {
                _Animations.Clear();
            }
        }

        private object _AnimationLock = new object();

        private Dictionary<UInt32, AnimationInfo> _Animations { get; } = new Dictionary<UInt32, AnimationInfo>();

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

            if (FrameNumber >= animationInfo.Faces.Length)
                return 0;

            return animationInfo.Faces[FrameNumber];
        }

        public int GetAnimationFrameCount(UInt16 AnimationNumber, int FrameNumber = 0)
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

            return animationInfo.Faces.Length;
        }

        private void _Handler_Animation(object sender, MessageHandler.AnimationEventArgs e)
        {
            _Logger.Info("Received Animation {0}", e.AnimationNumber);

            lock (_AnimationLock)
            {
                if (_Animations.ContainsKey(e.AnimationNumber))
                {
                    _Logger.Error("Animation already exists " + e.AnimationNumber);
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
        }
    }

    public class AnimationAvailableEventArgs : EventArgs
    {
        public UInt16 AnimationNumber { get; set; }
    }
}
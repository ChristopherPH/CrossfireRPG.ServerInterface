using CrossfireCore.ServerConfig;
using System;

namespace CrossfireCore.Managers.MapManagement
{
    public class MapAnimationState
    {
        public MapAnimationState()
        {
            ClearAnimation();
        }

        public MapAnimationState(Map.AnimationFlags animationFlags, int Speed, int FrameCount)
        {
            SetAnimation(animationFlags, Speed, FrameCount);
        }

        private Map.AnimationFlags _AnimationFlags;
        private int _AnimationSpeed;
        private int _AnimationFrameCount;

        public int CurrentTick { get; private set; } = 0;
        public int CurrentFrame { get; private set; } = 0;

        public void ClearAnimation()
        {
            SetAnimation(0, 0, 0);
            ResetState();
        }

        public void SetAnimation(Map.AnimationFlags animationFlags, int Speed, int FrameCount)
        {
            _AnimationFlags = animationFlags;
            _AnimationSpeed = Speed;
            _AnimationFrameCount = FrameCount;

            ResetState();
        }

        public void ResetState()
        {
            if (_AnimationFlags.HasFlag(Map.AnimationFlags.Randomize))
            {
                CurrentFrame = new Random().Next(0, _AnimationFrameCount);
                CurrentTick = new Random().Next(0, _AnimationSpeed);
            }
            else
            {
                CurrentTick = 0;
                CurrentFrame = 0;
            }
        }

        /// <summary>
        /// Updates the animation tick, and changes the frame when needed
        /// </summary>
        /// <returns>true if the frame changed</returns>
        public bool UpdateAnimation()
        {
            //Update tick until it rolls over the speed,
            //then set next frame
            CurrentTick++;
            if (CurrentTick < _AnimationSpeed)
                return false;

            CurrentTick = 0;

            //Randomize next frame
            if (_AnimationFlags.HasFlag(Map.AnimationFlags.Randomize))
            {
                var frame = new Random().Next(0, _AnimationFrameCount);

                if (CurrentFrame == frame)
                    return false;

                CurrentFrame = frame;
                return true;
            }

            //set next frame
            CurrentFrame++;

            if (CurrentFrame >= _AnimationFrameCount)
                CurrentFrame = 0;

            return true;
        }

        public MapAnimationState SaveMapAnimationState()
        {
            return new MapAnimationState()
            {
                _AnimationFlags = this._AnimationFlags,
                _AnimationSpeed = this._AnimationSpeed,
                _AnimationFrameCount = this._AnimationFrameCount,
                CurrentTick = this.CurrentTick,
                CurrentFrame = this.CurrentFrame,
            };
        }
    }
}

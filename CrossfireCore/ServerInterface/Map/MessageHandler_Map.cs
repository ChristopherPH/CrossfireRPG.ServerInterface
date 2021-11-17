using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageHandler
    {
        public event EventHandler<EventArgs> NewMap;
        public event EventHandler<MapEventArgs> Map;
        public event EventHandler<MapAnimationEventArgs> MapAnimation;
        public event EventHandler<MapDarknessEventArgs> MapDarkness;
        public event EventHandler<MapLocationEventArgs> MapClear;
        public event EventHandler<MapLocationLayerEventArgs> MapClearLayer;
#if THIS_IS_IN_THE_GTK_CLIENT
        public event EventHandler<MapLocationEventArgs> MapClearOld;
#endif
        public event EventHandler<MapLocationEventArgs> MapScroll;
        public event EventHandler<SmoothEventArgs> Smooth;

        protected override void HandleNewMap()
        {
            NewMap?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandleMap2Animation(int x, int y, int layer, ushort animation, int animationtype, byte animationspeed, byte smooth)
        {
            MapAnimation?.Invoke(this, new MapAnimationEventArgs()
            {
                X = x,
                Y = y,
                Layer = layer,
                Animation = animation,
                AnimationType = animationtype,
                AnimationSpeed = animationspeed,
                Smooth = smooth,
            });
        }

        protected override void HandleMap2Clear(int x, int y)
        {
            MapClear?.Invoke(this, new MapLocationEventArgs()
            {
                X = x,
                Y = y,
            });
        }

        protected override void HandleMap2ClearLayer(int x, int y, int layer)
        {
            MapClearLayer?.Invoke(this, new MapLocationLayerEventArgs()
            {
                X = x,
                Y = y,
                Layer = layer,
            });
        }

#if THIS_IS_IN_THE_GTK_CLIENT
        protected override void HandleMap2ClearOld(int x, int y)
        {
            MapClearOld?.Invoke(this, new MapLocationEventArgs()
            {
                X = x,
                Y = y,
            });
        }
#endif

        protected override void HandleMap2Darkness(int x, int y, byte darkness)
        {
            MapDarkness?.Invoke(this, new MapDarknessEventArgs()
            {
                X = x,
                Y = y,
                Darkness = darkness
            });
        }

        protected override void HandleMap2Face(int x, int y, int layer, ushort face, byte smooth)
        {
            Map?.Invoke(this, new MapEventArgs()
            {
                X = x,
                Y = y,
                Layer = layer,
                Face = face,
                Smooth = smooth
            });
        }

        protected override void HandleMap2Scroll(int x, int y)
        {
            MapScroll?.Invoke(this, new MapLocationEventArgs()
            {
                X = x,
                Y = y,
            });
        }

        protected override void HandleSmooth(ushort face, ushort smooth)
        {
            Smooth?.Invoke(this, new SmoothEventArgs()
            {
                Smooth = face,
                SmoothFace = smooth,
            });
        }

        public class MapEventArgs : SingleCommandEventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Layer { get; set; }
            public int Face { get; set; }
            public byte Smooth { get; set; }
        }

        public class MapAnimationEventArgs : SingleCommandEventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Layer { get; set; }
            public UInt16 Animation { get; set; }
            public int AnimationType { get; set; }
            public byte AnimationSpeed { get; set; }
            public byte Smooth { get; set; }
        }

        public class MapDarknessEventArgs : SingleCommandEventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }
            public byte Darkness { get; set; }
        }

        public class MapLocationEventArgs : SingleCommandEventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        public class MapLocationLayerEventArgs : SingleCommandEventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Layer { get; set; }
        }

        public class SmoothEventArgs : SingleCommandEventArgs
        {
            /// <summary>
            /// Face to Smooth
            /// </summary>
            public int Smooth { get; set; }

            /// <summary>
            /// Face to use when smoothing
            /// </summary>
            public Int64 SmoothFace { get; set; }
        }
    }
}

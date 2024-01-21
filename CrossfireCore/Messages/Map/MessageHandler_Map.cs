using System;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageHandler
    {
        public event EventHandler<EventArgs> NewMap;
        public event EventHandler<EventArgs> MapBegin;
        public event EventHandler<EventArgs> MapEnd;
        public event EventHandler<MapLocationEventArgs> MapBeginLocation;
        public event EventHandler<MapLocationEventArgs> MapEndLocation;
        public event EventHandler<MapFaceEventArgs> MapFace;
        public event EventHandler<MapAnimationEventArgs> MapAnimation;
        public event EventHandler<MapDarknessEventArgs> MapDarkness;
        public event EventHandler<MapLocationEventArgs> MapClear;
        public event EventHandler<MapLocationLayerEventArgs> MapClearLayer;
        public event EventHandler<MapLocationEventArgs> MapScroll;
        public event EventHandler<SmoothEventArgs> Smooth;

        protected override void HandleNewMap()
        {
            NewMap?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandleMap2Begin()
        {
            MapBegin?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandleMap2End()
        {
            MapEnd?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandleMap2BeginLocation(int x, int y)
        {
            MapBeginLocation?.Invoke(this, new MapLocationEventArgs()
            {
                X = x,
                Y = y,
            });
        }

        protected override void HandleMap2EndLocation(int x, int y)
        {
            MapEndLocation?.Invoke(this, new MapLocationEventArgs()
            {
                X = x,
                Y = y,
            });
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
            MapFace?.Invoke(this, new MapFaceEventArgs()
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

        public class MapFaceEventArgs : MessageHandlerEventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Layer { get; set; }
            public int Face { get; set; }
            public byte Smooth { get; set; }
        }

        public class MapAnimationEventArgs : MessageHandlerEventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Layer { get; set; }
            public UInt16 Animation { get; set; }
            public int AnimationType { get; set; }
            public byte AnimationSpeed { get; set; }
            public byte Smooth { get; set; }
        }

        public class MapDarknessEventArgs : MessageHandlerEventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }
            public byte Darkness { get; set; }
        }

        public class MapLocationEventArgs : MessageHandlerEventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        public class MapLocationLayerEventArgs : MessageHandlerEventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Layer { get; set; }
        }

        public class SmoothEventArgs : MessageHandlerEventArgs
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

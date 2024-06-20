#define MAPOBJECT_SERIALIZATION
using CrossfireCore.ServerConfig;
using System;
using System.Xml.Serialization;

namespace CrossfireCore.Managers.MapManagement
{

    //TODO: Consider moving to a struct, as this
    //      already provides an equals and ==
    public class MapLayer
    {
        [XmlAttribute]
        public int Face { get; set; } = 0;

        /// <summary>
        /// 0 (overlap nothing) to 255 (overlap above everything except other objects having also smoothlevel of 255)
        /// </summary>
        [XmlAttribute]
        public int SmoothLevel { get; set; } = 0;

        [XmlAttribute]
        public UInt16 Animation { get; set; } = 0;

        [XmlAttribute]
        public Map.AnimationFlags AnimationFlags { get; set; } = Map.AnimationFlags.Normal;

        [XmlAttribute]
        public byte AnimationSpeed { get; set; } = 0;

        [XmlIgnore]
        public MapAnimationState AnimationState { get; } = new MapAnimationState();

        [XmlIgnore]
        public bool IsAnimation => Animation != 0;

        public void ClearLayer()
        {
            Face = 0;
            SmoothLevel = 0;
            Animation = 0;
            AnimationFlags = Map.AnimationFlags.Normal;
            AnimationSpeed = 0;

            AnimationState.ClearAnimation();
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return Equals(obj as MapLayer);
        }

        public bool Equals(MapLayer other)
        {
            /* Compare properties common between
             * animation and face layers */
            if ((IsAnimation != other.IsAnimation) ||
                (SmoothLevel != other.SmoothLevel))
            {
                return false;
            }

            /* If the layers are both animations,
             * then need to compare the type and speed */
            if (IsAnimation)
            {
                return
                    Animation == other.Animation &&
                    AnimationFlags == other.AnimationFlags &&
                    AnimationSpeed == other.AnimationSpeed;
            }
            else
            {
                return Face == other.Face;
            }
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Tuple.Create(Face, SmoothLevel, Animation, AnimationFlags, AnimationSpeed).GetHashCode();
        }

        public static bool operator ==(MapLayer x, MapLayer y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;

            return x.Equals(y);
        }

        public static bool operator !=(MapLayer x, MapLayer y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            if (IsAnimation)
                return $"Anim: {Animation}";
            else
                return $"Face: {Face}";
        }

        public MapLayer SaveLayer()
        {
            return new MapLayer()
            {
                Face = this.Face,
                SmoothLevel = this.SmoothLevel,
                Animation = this.Animation,
                AnimationFlags = this.AnimationFlags,
                AnimationSpeed = this.AnimationSpeed,
            };

            //TODO: Consider also saving the state
            //AnimationState = this.AnimationState.SaveMapAnimationState(),
        }
    }
}

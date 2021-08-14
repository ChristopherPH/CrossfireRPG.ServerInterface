using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire
{
    public class Parser : CommandParser
    {
        public event EventHandler<VersionEventArgs> Version;
        public event EventHandler<EventArgs> NewMap;
        public event EventHandler<EventArgs> Goodbye;
        public event EventHandler<EventArgs> AddmeFailed;
        public event EventHandler<EventArgs> AddmeSuccess;
        public event EventHandler<AnimationEventArgs> Animation;
        public event EventHandler<DrawExtInfoEventArgs> DrawExtInfo;
        public event EventHandler<Image2EventArgs> Image2;

        protected override void HandleAddmeFailed()
        {
            AddmeFailed?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandleAddmeSuccess()
        {
            AddmeSuccess?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandleAnimation(ushort AnimationNumber, ushort AnimationFlags, ushort[] AnimationFaces)
        {
            Animation?.Invoke(this, new AnimationEventArgs()
            {
                AnimationNumber = AnimationNumber,
                AnimationFlags = AnimationFlags,
                AnimationFaces = AnimationFaces,
            });
        }

        protected override void HandleDeleteInventory(int ObjectTag)
        {
        }

        protected override void HandleDrawExtInfo(NewClient.NewDrawInfo Colour, NewClient.MsgTypes MessageType, int SubType, string Message)
        {
            DrawExtInfo?.Invoke(this, new DrawExtInfoEventArgs()
            {
                Colour = Colour,
                MessageType = MessageType,
                SubType = SubType,
                Message = Message
            });
        }

        protected override void HandleFailure(string ProtocolCommand, string FailureString)
        {
        }

        protected override void HandleGoodbye()
        {
            Goodbye?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandleImage2(uint image_face, byte image_faceset, byte[] image_png)
        {
            Image2?.Invoke(this, new Image2EventArgs()
            {
                ImageFace = image_face,
                ImageFaceSet = image_faceset,
                ImageData = image_png
            });
        }

        protected override void HandleItem2(uint item_location, uint item_tag, uint item_flags, uint item_weight, uint item_face, string item_name, ushort item_anim, byte item_animspeed, uint item_nrof, ushort item_type)
        {
        }

        protected override void HandleMap2Animation(int x, int y, int layer, ushort animation, int animationtype, byte animationspeed, byte smooth)
        {
        }

        protected override void HandleMap2Clear(int x, int y)
        { 
        }

        protected override void HandleMap2ClearAnimationSmooth(int x, int y, int layer)
        {
        }

        protected override void HandleMap2Darkness(int x, int y, byte darkness)
        {
        }

        protected override void HandleMap2Face(int x, int y, int layer, ushort face, byte smooth)
        {
        }

        protected override void HandleNewMap()
        {
            NewMap?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandleQuery(int Flags, string QueryText)
        {
        }

        protected override void HandleSkill(int Skill, long Value)
        {
        }

        protected override void HandleSmooth(ushort face, ushort smooth)
        {
        }

        protected override void HandleStat(NewClient.CharacterStats Stat, long Value)
        {
        }

        protected override void HandleStat(NewClient.CharacterStats Stat, string Value)
        {
        }

        protected override void HandleStat(NewClient.CharacterStats Stat, float Value)
        {
        }

        protected override void HandleVersion(int csval, int scval, string verstring)
        {
            Version?.Invoke(this, new VersionEventArgs()
            {
                csval = csval,
                scval = scval,
                verstring = verstring,
            });
        }

        public class VersionEventArgs : EventArgs
        {
            public int csval { get; set; }
            public int scval { get; set; }
            public string verstring { get; set; }
        }

        public class AnimationEventArgs : EventArgs
        {
            public ushort AnimationNumber { get; set; }
            public ushort AnimationFlags { get; set; }
            public ushort[] AnimationFaces { get; set; }
        }

        public class DrawExtInfoEventArgs : EventArgs
        {
            public NewClient.NewDrawInfo Colour { get; set; }
            public NewClient.MsgTypes MessageType { get; set; }
            public int SubType { get; set; }
            public string Message { get; set; }
        }

        public class Image2EventArgs
        {
            public UInt32 ImageFace { get; set; }
            public byte ImageFaceSet { get; set; }
            public byte[] ImageData { get; set; }
        }
    }
}

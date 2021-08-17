using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crossfire.ServerInterface;

namespace Crossfire
{
    public class Parser : CommandParser
    {
        public event EventHandler<VersionEventArgs> Version;
        public event EventHandler<EventArgs> NewMap;
        public event EventHandler<EventArgs> Goodbye;
        public event EventHandler<PlayerEventArgs> Player;
        public event EventHandler<EventArgs> AddmeFailed;
        public event EventHandler<EventArgs> AddmeSuccess;
        public event EventHandler<AnimationEventArgs> Animation;
        public event EventHandler<DrawExtInfoEventArgs> DrawExtInfo;
        public event EventHandler<Image2EventArgs> Image2;
        public event EventHandler<Item2EventArgs> Item2;
        public event EventHandler<DeleteItemEventArgs> DeleteItem;
        public event EventHandler<DeleteInventoryEventArgs> DeleteInventory;
        public event EventHandler<StatEventArgs> Stats;
        public event EventHandler<SkillEventArgs> Skills;
        public event EventHandler<MapEventArgs> Map;
        public event EventHandler<SmoothEventArgs> Smooth;
        public event EventHandler<AccountPlayerEventArgs> AccountPlayer;
        public event EventHandler<FailureEventArgs> Failure;
        public event EventHandler<SetupEventArgs> Setup;

        protected override void HandleAccountPlayer(int PlayerCount, int PlayerNumber, string PlayerName)
        {
            AccountPlayer?.Invoke(this, new AccountPlayerEventArgs()
            {
                PlayerCount = PlayerCount,
                PlayerNumber = PlayerNumber,
                PlayerName = PlayerName
            });
        }

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

        protected override void HandleComC(ushort comc_packet, uint comc_time)
        {
            System.Diagnostics.Debug.Print("Finished Command {0}:{1}", comc_packet, comc_time);
        }

        protected override void HandleDeleteInventory(int ObjectTag)
        {
            DeleteInventory?.Invoke(this, new DeleteInventoryEventArgs()
            {
                ObjectTag = ObjectTag
            });
        }

        protected override void HandleDeleteItem(uint ObjectTag)
        {
            DeleteItem?.Invoke(this, new DeleteItemEventArgs()
            {
                ObjectTag = ObjectTag
            });
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
            Failure?.Invoke(this, new FailureEventArgs()
            {
                ProtocolCommand = ProtocolCommand, 
                FailureString = FailureString
            });
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

        protected override void HandleItem2(uint item_location, uint item_tag, uint item_flags, 
            uint item_weight, uint item_face, string item_name, string item_name_plural,
            ushort item_anim, byte item_animspeed, uint item_nrof, ushort item_type)
        {
            Item2?.Invoke(this, new Item2EventArgs()
            {
                item_location = item_location,
                item_tag = item_tag,
                item_flags = item_flags,
                item_weight = item_weight,
                item_face = item_face,
                item_name = item_name,
                item_name_plural = item_name_plural,
                item_anim = item_anim,
                item_animspeed = item_animspeed,
                item_nrof = item_nrof,
                item_type = item_type,
            });
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
            Map?.Invoke(this, new MapEventArgs()
            {
                X = x,
                Y = y,
                Layer = layer,
                Face = face,
                Smooth = smooth
            });
        }

        protected override void HandleNewMap()
        {
            NewMap?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandlePlayer(uint tag, uint weight, uint face, string Name)
        {
            Player?.Invoke(this, new PlayerEventArgs()
            {
                tag = tag,
                weight = weight,
                face = face,
                PlayerName = Name
            });
        }

        protected override void HandleQuery(int Flags, string QueryText)
        {
        }

        protected override void HandleSetup(string SetupCommand, string SetupValue)
        {
            Setup?.Invoke(this, new SetupEventArgs()
            {
                SetupCommand = SetupCommand,
                SetupValue = SetupValue,
            });
        }

        protected override void HandleSkill(int Skill, byte Level, UInt64 Value)
        {
            Skills?.Invoke(this, new SkillEventArgs()
            {
                Skill = Skill,
                Level = Level,
                Value = Value,
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

        protected override void HandleStat(NewClient.CharacterStats Stat, long Value)
        {
            Stats?.Invoke(this, new StatEventArgs()
            {
                Stat = Stat,
                Value = Value.ToString(),
            });
        }

        protected override void HandleStat(NewClient.CharacterStats Stat, string Value)
        {
            Stats?.Invoke(this, new StatEventArgs()
            {
                Stat = Stat,
                Value = Value,
            });
        }

        protected override void HandleStat(NewClient.CharacterStats Stat, float Value)
        {
            Stats?.Invoke(this, new StatEventArgs()
            {
                Stat = Stat,
                Value = Value.ToString(),
            });
        }

        protected override void HandleVersion(int csval, int scval, string verstring)
        {
            Version?.Invoke(this, new VersionEventArgs()
            {
                ClientToServerProtocolVersion = csval,
                ServerToClientProtocolVersion = scval,
                ClientVersionString = verstring,
            });
        }

        public class VersionEventArgs : EventArgs
        {
            public int ClientToServerProtocolVersion { get; set; }
            public int ServerToClientProtocolVersion { get; set; }
            public string ClientVersionString { get; set; }
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

        public class Image2EventArgs : EventArgs
        {
            public UInt32 ImageFace { get; set; }
            public byte ImageFaceSet { get; set; }
            public byte[] ImageData { get; set; }
        }

        public class StatEventArgs : EventArgs
        {
            public NewClient.CharacterStats Stat { get; set; }
            public string Value { get; set; }
        }
        public class SkillEventArgs : EventArgs
        {
            public int Skill { get; set; }
            public Byte Level { get; set; }
            public UInt64 Value { get; set; }
        }

        public class SmoothEventArgs : EventArgs
        {
            public int Smooth { get; set; }
            public Int64 SmoothFace { get; set; }
        }

        public class MapEventArgs : EventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Layer { get; set; }
            public int Face { get; set; }
            public int Smooth { get; set; }
        }

        public class AccountPlayerEventArgs : EventArgs
        {
            public int PlayerCount { get; set; }
            public int PlayerNumber { get; set; }
            public string PlayerName { get; set; }
        }

        public class FailureEventArgs : EventArgs
        {
            public string ProtocolCommand { get; set; }
            public string FailureString { get; set; }
        }

        public class Item2EventArgs : EventArgs
        {
            public UInt32 item_location { get; set; }
            public UInt32 item_tag { get; set; }
            public UInt32 item_flags { get; set; }
            public UInt32 item_weight { get; set; }
            public UInt32 item_face { get; set; }
            public string item_name { get; set; }
            public string item_name_plural { get; set; }
            public UInt16 item_anim { get; set; }
            public byte item_animspeed { get; set; }
            public UInt32 item_nrof { get; set; }
            public UInt16 item_type { get; set; }
        }

        public class DeleteItemEventArgs : EventArgs
        {
            public UInt32 ObjectTag { get; set; }
        }

        public class DeleteInventoryEventArgs : EventArgs
        {
            public int ObjectTag { get; set; }
        }

        public class PlayerEventArgs : EventArgs
        {
            public UInt32 tag { get; set; }
            public UInt32 weight { get; set; }
            public UInt32 face { get; set; }
            public string PlayerName { get; set; }
        }

        public class SetupEventArgs : EventArgs
        {
            public string SetupCommand { get; set; }
            public string SetupValue { get; set; }
        }
    }
}

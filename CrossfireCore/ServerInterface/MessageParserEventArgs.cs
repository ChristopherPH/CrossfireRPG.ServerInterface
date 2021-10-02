using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    //TODO: remove thsese from MessageParserBase, but should they be in their own namespace?
    public abstract partial class MessageParserBase //TODO: remove this line and matching end brace
    {
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
            public NewClient.NewDrawInfo Flags { get; set; }
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
            /// <summary>
            /// Face to Smooth
            /// </summary>
            public int Smooth { get; set; }

            /// <summary>
            /// Face to use when smoothing
            /// </summary>
            public Int64 SmoothFace { get; set; }
        }

        public class MapEventArgs : EventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Layer { get; set; }
            public int Face { get; set; }
            public byte Smooth { get; set; }
        }

        public class MapLocationEventArgs : EventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        public class MapLocationLayerEventArgs : EventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Layer { get; set; }
        }

        public class MapDarknessEventArgs : EventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }
            public byte Darkness { get; set; }
        }

        public class MapAnimationEventArgs : EventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Layer { get; set; }
            public UInt16 Animation { get; set; }
            public int AnimationType { get; set; }
            public byte AnimationSpeed { get; set; }
            public byte Smooth { get; set; }
        }

        public class AccountPlayerEventArgs : EventArgs
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

        public class UpdateItemEventArgs : EventArgs
        {
            public UInt32 ObjectTag { get; set; }
            public NewClient.UpdateTypes UpdateType { get; set; }
            public Int64 UpdateValue { get; set; }
            public string UpdateString { get; set; }
        }


        public class ReplyInfoEventArgs : EventArgs
        {
            public string Request { get; set; }
            public byte[] Reply { get; set; }
        }

        public class QueryEventArgs : EventArgs
        {
            public int Flags { get; set; }
            public string QueryText { get; set; }
        }

        public class AddSpellEventArgs : EventArgs
        {
            public UInt32 SpellTag { get; set; }
            public Int16 Level { get; set; }
            public Int16 CastingTime { get; set; }
            public Int16 Mana { get; set; }
            public Int16 Grace { get; set; }
            public Int16 Damage { get; set; }
            public byte Skill { get; set; }
            public UInt32 Path { get; set; }
            public Int32 Face { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public byte Usage { get; set; }
            public string Requirements { get; set; }
        }

        public class UpdateSpellEventArgs : EventArgs
        {
            public UInt32 SpellTag { get; set; }
            public NewClient.UpdateSpellTypes UpdateType { get; set; }
            public Int64 UpdateValue { get; set; }
        }

        public class DeleteSpellEventArgs : EventArgs
        {
            public UInt32 SpellTag { get; set; }
        }

        public class AddQuestEventArgs : EventArgs
        {
            public UInt32 Code { get; set; }
            public string Title { get; set; }
            public Int32 Face { get; set; }
            public byte Replay { get; set; }
            public UInt32 Parent { get; set; }
            public byte End { get; set; }
            public string Step { get; set; }
        }

        public class UpdateQuestEventArgs : EventArgs
        {
            public UInt32 Code { get; set; }
            public byte End { get; set; }
            public string Step { get; set; }
        }

        public class AddKnowledgeEventArgs : EventArgs
        {
            public UInt32 ID { get; set; }
            public string Type { get; set; }
            public string Title { get; set; }
            public Int32 Face { get; set; }
        }

        public class PickupEventArgs : EventArgs
        {
            public UInt32 Flags { get; set; }
        }

        public class CompletedCommandEventArgs : EventArgs
        {
            public UInt16 Packet { get; set; }
            public UInt32 Time { get; set; }
        }

        public class TickEventArgs : EventArgs
        {
            public UInt32 TickCount { get; set; }
        }
    }
}

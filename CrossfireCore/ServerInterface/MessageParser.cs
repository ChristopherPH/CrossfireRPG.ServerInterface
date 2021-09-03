using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossfireCore.ServerInterface
{
    public class MessageParser : MessageParserBase
    {
        public MessageParser(SocketConnection Connection) : base(Connection) { }

        public event EventHandler<EventArgs> AddmeFailed;
        public event EventHandler<EventArgs> AddmeSuccess;
        public event EventHandler<AnimationEventArgs> Animation;
        public event EventHandler<DrawExtInfoEventArgs> DrawExtInfo;
        public event EventHandler<EventArgs> Goodbye;
        public event EventHandler<Image2EventArgs> Image2;
        public event EventHandler<Item2EventArgs> Item2;
        public event EventHandler<EventArgs> NewMap;
        public event EventHandler<PlayerEventArgs> Player;
        public event EventHandler<UpdateItemEventArgs> UpdateItem;
        public event EventHandler<DeleteItemEventArgs> DeleteItem;
        public event EventHandler<DeleteInventoryEventArgs> DeleteInventory;
        public event EventHandler<StatEventArgs> Stats;
        public event EventHandler<SkillEventArgs> Skills;
        public event EventHandler<MapEventArgs> Map;
        public event EventHandler<MapLocationLayerEventArgs> MapClearAnimationSmooth;
        public event EventHandler<MapLocationEventArgs> MapClear;
        public event EventHandler<MapLocationEventArgs> MapClearOld;
        public event EventHandler<MapLocationEventArgs> MapScroll;
        public event EventHandler<MapAnimationEventArgs> MapAnimation;
        public event EventHandler<MapDarknessEventArgs> MapDarkness;
        public event EventHandler<SmoothEventArgs> Smooth;
        public event EventHandler<AccountPlayerEventArgs> AccountPlayer;
        public event EventHandler<FailureEventArgs> Failure;
        public event EventHandler<SetupEventArgs> Setup;
        public event EventHandler<ReplyInfoEventArgs> ReplyInfo;
        public event EventHandler<VersionEventArgs> Version;
        public event EventHandler<QueryEventArgs> Query;
        public event EventHandler<AddSpellEventArgs> AddSpell;
        public event EventHandler<UpdateSpellEventArgs> UpdateSpell;
        public event EventHandler<DeleteSpellEventArgs> DeleteSpell;
        public event EventHandler<AddQuestEventArgs> AddQuest;
        public event EventHandler<UpdateQuestEventArgs> UpdateQuest;
        public event EventHandler<AddKnowledgeEventArgs> AddKnowledge;
        public event EventHandler<PickupEventArgs> Pickup;

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

        protected override void HandleMap2ClearAnimationSmooth(int x, int y, int layer)
        {
            MapClearAnimationSmooth?.Invoke(this, new MapLocationLayerEventArgs()
            {
                X = x,
                Y = y,
                Layer = layer,
            });
        }

        protected override void HandleMap2ClearOld(int x, int y)
        {
            MapClearOld?.Invoke(this, new MapLocationEventArgs()
            {
                X = x,
                Y = y,
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
            Query?.Invoke(this, new QueryEventArgs()
            {
                Flags = Flags,
                QueryText = QueryText
            });
        }

        protected override void HandleReplyInfo(string request, byte[] reply)
        {
            ReplyInfo?.Invoke(this, new ReplyInfoEventArgs()
            {
                Request = request,
                Reply = reply
            });
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

        protected override void HandleUpdateItem(UInt32 ObjectTag, NewClient.UpdateTypes UpdateType, long UpdateValue)
        {
            UpdateItem?.Invoke(this, new UpdateItemEventArgs()
            {
                ObjectTag = ObjectTag,
                UpdateType = UpdateType,
                UpdateValue = UpdateValue
            });
        }

        protected override void HandleUpdateItem(UInt32 ObjectTag, NewClient.UpdateTypes UpdateType, string UpdateValue)
        {
            UpdateItem?.Invoke(this, new UpdateItemEventArgs()
            {
                ObjectTag = ObjectTag,
                UpdateType = UpdateType,
                UpdateString = UpdateValue
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

        protected override void HandleAddSpell(uint SpellTag, short Level, short CastingTime,
                    short Mana, short Grace, short Damage, byte Skill, uint Path, int Face,
                    string Name, string Description, byte Usage, string Requirements)
        {
            AddSpell?.Invoke(this, new AddSpellEventArgs()
            {
                SpellTag = SpellTag,
                Level = Level,
                CastingTime = CastingTime,
                Mana = Mana,
                Grace = Grace,
                Damage = Damage,
                Skill = Skill,
                Path = Path,
                Face = Face,
                Name = Name,
                Description = Description,
                Usage = Usage,
                Requirements = Requirements
            });
        }

        protected override void HandleUpdateSpell(UInt32 SpellTag, NewClient.UpdateSpellTypes UpdateType, Int64 UpdateValue)
        {
            UpdateSpell?.Invoke(this, new UpdateSpellEventArgs()
            {
                SpellTag = SpellTag,
                UpdateType = UpdateType,
                UpdateValue = UpdateValue
            });
        }

        protected override void HandleDeleteSpell(UInt32 SpellTag)
        {
            DeleteSpell?.Invoke(this, new DeleteSpellEventArgs()
            {
                SpellTag = SpellTag,
            });
        }

        protected override void HandleAddQuest(UInt32 Code, string Title, int Face, 
            byte Replay, uint Parent, byte End, string Step)
        {
            AddQuest?.Invoke(this, new AddQuestEventArgs()
            {
                Code = Code,
                Title = Title,
                Face = Face,
                Replay = Replay,
                Parent = Parent, 
                End = End,
                Step = Step,
            });
        }

        protected override void HandleUpdateQuest(UInt32 Code, byte End, string Step)
        {
            UpdateQuest?.Invoke(this, new UpdateQuestEventArgs()
            {
                Code = Code,
                End = End,
                Step = Step,
            });
        }

        protected override void HandleAddKnowledge(UInt32 ID, string Type, string Title, Int32 Face)
        {
            AddKnowledge?.Invoke(this, new AddKnowledgeEventArgs()
            {
                ID = ID,
                Type = Type,
                Title = Title,
                Face = Face
            });
        }

        protected override void HandlePickup(UInt32 PickupFlags)
        {
            Pickup?.Invoke(this, new PickupEventArgs()
            {
                Flags = PickupFlags,
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
    }
}

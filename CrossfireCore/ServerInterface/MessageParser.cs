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
        public event EventHandler<MapLocationLayerEventArgs> MapClearLayer;
        public event EventHandler<MapLocationEventArgs> MapClear;
#if THIS_IS_IN_THE_GTK_CLIENT
        public event EventHandler<MapLocationEventArgs> MapClearOld;
#endif
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
        public event EventHandler<TickEventArgs> Tick;
        public event EventHandler<CompletedCommandEventArgs> CompletedCommand;

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

        protected override void HandleCompletedCommand(ushort comc_packet, uint comc_time)
        {
            CompletedCommand?.Invoke(this, new CompletedCommandEventArgs()
            {
                Packet = comc_packet,
                Time = comc_time,
            });
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

        protected override void HandleTick(UInt32 TickCount)
        {
            Tick?.Invoke(this, new TickEventArgs()
            {
                TickCount = TickCount,
            });
        }
    }
}

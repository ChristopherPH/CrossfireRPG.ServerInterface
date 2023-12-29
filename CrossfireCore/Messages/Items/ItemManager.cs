using Common;
using Common.Utility;
using CrossfireCore.ServerConfig;
using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossfireCore.Managers
{
    public class ItemManager : DataListManager<Item>
    {
        public ItemManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Handler.Item2 += _Handler_Item2;
            Handler.UpdateItem += _Handler_UpdateItem;
            Handler.DeleteItem += _Handler_DeleteItem;
            Handler.DeleteInventory += _Handler_DeleteInventory;
            Handler.Player += _Handler_Player;
            Handler.BeginItem2 += _Handler_BeginItem2;
            Handler.EndItem2 += _Handler_EndItem2;
            Handler.BeginDeleteItem += _Handler_BeginDeleteItem;
            Handler.EndDeleteItem += _Handler_EndDeleteItem;
            Handler.BeginUpdateItem += _Handler_BeginUpdateItem;
            Handler.EndUpdateItem += _Handler_EndUpdateItem;
        }

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => true;
        public override ModificationTypes SupportedModificationTypes => base.SupportedModificationTypes |
            ModificationTypes.Added | ModificationTypes.Updated | ModificationTypes.Removed |
            ModificationTypes.MultiCommandStart | ModificationTypes.MultiCommandEnd;

        private UInt32 _PlayerTag = 0;
        static Logger _Logger = new Logger(nameof(ItemManager));

        public Item OpenContainer { get; private set; } = null;
        public bool HasOpenContainer => OpenContainer != null;

        public IEnumerable<Item> PlayerItems => this.Where(x => x.Location == ItemLocations.Player);
        public IEnumerable<Item> PlayerHeldItems => PlayerItems.Where(x => !x.IsInContainer);
        public IEnumerable<Item> PlayerContainedItems => PlayerItems.Where(x => x.IsInContainer);
        public IEnumerable<Item> PlayerActiveContainers => PlayerItems.Where(x => x.ApplyType == NewClient.ItemFlags.Active);
        public IEnumerable<Item> GroundItems => this.Where(x => x.IsOnGround);
        public IEnumerable<Item> ContainedItems => this.Where(x => x.IsInContainer);
        public IEnumerable<Item> OpenContainerItems => (OpenContainer == null) ? Enumerable.Empty<Item>() :
            ContainedItems.Where(x => x.LocationTag == OpenContainer.Tag);

        public Item GetItemByTag(UInt32 ItemTag)
        {
            return GetData(x => x.Tag == ItemTag);
        }

        public event EventHandler<ContainerModifiedEventArgs> ContainerChanged;

        protected void OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes ChangeType, Item Item, int ItemIndex)
        {
            ContainerChanged?.Invoke(this, new ContainerModifiedEventArgs()
            {
                Modification = ChangeType,
                Item = Item,
                ItemIndex = ItemIndex
            });
        }

        protected override void ClearData()
        {
            if (OpenContainer != null)
            {
                OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Closed,
                    OpenContainer, this.GetIndex(OpenContainer));

                OpenContainer = null;
            }

            //Log what items are being cleared. Technically the server should be deleting
            //all player items between characters, but it doesn't seem to get all of them
            if (this.Count > 0)
            {
                _Logger.Debug("Begin clear ItemManager");

                foreach (var item in this)
                    _Logger.Debug("Clearing {0}", item);

                _Logger.Debug("End clear ItemManager");
            }

            base.ClearData();
        }

        private void _Handler_Item2(object sender, MessageHandler.Item2EventArgs e)
        {
            var item = new Item()
            {
                LocationTag = e.item_location,
                Tag = e.item_tag,
                Flags = (NewClient.ItemFlags)e.item_flags,
                Weight = e.item_weight,
                Face = e.item_face,
                Name = e.item_name,
                NamePlural = e.item_name_plural,
                Animation = e.item_anim,
                AnimationSpeed = e.item_animspeed,
                RawNumberOf = e.item_nrof,
                ClientType = e.item_type,
            };

            item.Location = GetLocation(item.LocationTag, out var inContainer);
            item.IsInContainer = inContainer;

            var ix = this.GetIndex(x => x.Tag == e.item_tag, out var existingItem);
            if (ix != -1)
            {
                var UpdatedProperties = new List<string>();

                if (item.LocationTag != existingItem.LocationTag)
                {
                    UpdatedProperties.Add(nameof(Item.LocationTag)); //LocationTag changes when picking up an item
                    UpdatedProperties.Add(nameof(Item.Location));
                }
                if (item.IsInContainer != existingItem.IsInContainer)
                    UpdatedProperties.Add(nameof(Item.IsInContainer));
                if (item.Flags != existingItem.Flags)
                    UpdatedProperties.Add(nameof(Item.Flags));
                if (item.Weight != existingItem.Weight)
                    UpdatedProperties.Add(nameof(Item.Weight));
                if (item.Face != existingItem.Face)
                    UpdatedProperties.Add(nameof(Item.Face));
                if (item.Name != existingItem.Name)
                    UpdatedProperties.Add(nameof(Item.Name));
                if (item.NamePlural != existingItem.NamePlural)
                    UpdatedProperties.Add(nameof(Item.NamePlural));
                if (item.Animation != existingItem.Animation)
                    UpdatedProperties.Add(nameof(Item.Animation));
                if (item.AnimationSpeed != existingItem.AnimationSpeed)
                    UpdatedProperties.Add(nameof(Item.AnimationSpeed));
                if (item.RawNumberOf != existingItem.RawNumberOf)
                {
                    UpdatedProperties.Add(nameof(Item.RawNumberOf));
                    UpdatedProperties.Add(nameof(Item.NumberOf));
                }
                if (item.ClientType != existingItem.ClientType)
                    UpdatedProperties.Add(nameof(Item.ClientType));

                _Logger.Warning("Trying to add existing object {0}, updating instead: {1}", e.item_tag,
                    string.Join(", ", UpdatedProperties));

                UpdateData(ix, item);
            }
            else
            {
                ix = AddData(item);
            }

            _Logger.Info("Added {0}", item);

            if (item.IsOpen && (item != OpenContainer)) //avoid issue with picking up an open container
            {
                if (OpenContainer != null)
                {
                    _Logger.Warning("Container {0} was already open when received new open container {1}", OpenContainer, item);

                    OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Closed,
                        OpenContainer, this.GetIndex(OpenContainer));
                }

                OpenContainer = item;
                OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Opened,
                        item, ix);
            }
        }

        private void _Handler_DeleteItem(object sender, MessageHandler.DeleteItemEventArgs e)
        {
            var ix = this.GetIndex(x => x.Tag == e.ObjectTag, out var item);
            if (ix == -1)
            {
                //We will get these messages when applying a bed to reality, when we have
                //applied containers with items inside, that we haven't opened in this session.
                //The server has not sent the items to the client, but sends a removal message
                //when quitting.
                //We will also get these messages when deleting a character, as the player list
                //is sent before the deletion of inventory. The item manager will be cleared on
                //a seeing the player list.
                _Logger.Warning("Trying to delete invalid object {0}", e.ObjectTag);
                return;
            }

            if (item == OpenContainer)
            {
                OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Closed,
                    item, ix);
                OpenContainer = null;
            }

            _Logger.Info("Deleted {0}", item);

            this.RemoveData(ix);
        }

        private void _Handler_DeleteInventory(object sender, MessageHandler.DeleteInventoryEventArgs e)
        {
            var location = string.Empty;

            if ((_PlayerTag > 0) && (e.ObjectTag == _PlayerTag))
            {
                location = string.Format("player ({0})", e.ObjectTag);
            }
            else if (e.ObjectTag == 0)
            {
                location = string.Format("ground ({0})", e.ObjectTag);
            }
            else
            {
                var item = GetItemByTag((UInt32)e.ObjectTag);
                if (item == null)
                    location = string.Format("missing item ({0})", e.ObjectTag);
                else
                    location = item.ToString();
            }

            _Logger.Info("Deleting inventory of {0}", location);

            var items = this.Where(x => x.LocationTag == (uint)e.ObjectTag).ToList();
            if (items.Count > 0)
            {
                _Logger.Debug("Begin delete inventory");
                this.StartMultiCommand();

                foreach (var item in items)
                {
                    _Logger.Debug("Deleted {0} from {1}", item, location);

                    if (item == OpenContainer)
                    {
                        OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Closed,
                            item, this.GetIndex(item));
                        OpenContainer = null;
                    }

                    this.RemoveData(item);
                }

                _Logger.Debug("End delete inventory");
                this.EndMultiCommand();
            }
        }

        private void _Handler_UpdateItem(object sender, MessageHandler.UpdateItemEventArgs e)
        {
           if ((_PlayerTag > 0) && (e.ObjectTag == _PlayerTag))
                return;

            var ix = this.GetIndex(x => x.Tag == e.ObjectTag, out var item);
            if (ix == -1)
            {
                _Logger.Warning("Trying to update invalid object {0}", e.ObjectTag);
                return;
            }

            _Logger.Info("Update {0} of {1} to ({2}/{3}/{4}/{5}) {6} {7}",
                e.UpdateType, item, e.UpdateValueUInt8, e.UpdateValueUInt16, e.UpdateValueUInt32,
                e.UpdateValueFloat, e.UpdateString, e.UpdateStringPlural);

            string[] UpdatedProperties = null;

            switch (e.UpdateType)
            {
                case NewClient.UpdateTypes.Location:
                    if (e.DataType == MessageHandler.UpdateItemEventArgs.UpdateDataTypes.UInt32)
                    {
                        if (item.LocationTag != e.UpdateValueUInt32)
                        {
                            item.LocationTag = e.UpdateValueUInt32;

                            item.Location = GetLocation(item.LocationTag, out var inContainer);
                            item.IsInContainer = inContainer;
                            UpdatedProperties = new string[] { nameof(Item.LocationTag),
                            nameof(Item.Location), nameof(Item.IsInContainer) };
                        }
                    }
                    else
                    {
                        _Logger.Warning("UpdateTypes.Location has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.UpdateTypes.Flags:
                    if (e.DataType == MessageHandler.UpdateItemEventArgs.UpdateDataTypes.UInt32)
                    {
                        if (item.Flags != (NewClient.ItemFlags)e.UpdateValueUInt32)
                        {
                            item.Flags = (NewClient.ItemFlags)e.UpdateValueUInt32;
                            UpdatedProperties = new string[] { nameof(Item.Flags) };
                        }
                    }
                    else
                    {
                        _Logger.Warning("UpdateTypes.Flags has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.UpdateTypes.Weight:
                    if (e.DataType == MessageHandler.UpdateItemEventArgs.UpdateDataTypes.Float)
                    {
                        if (item.Weight != e.UpdateValueFloat)
                        {
                            item.Weight = (UInt32)e.UpdateValueFloat;
                            UpdatedProperties = new string[] { nameof(Item.Weight) };
                        }
                    }
                    else
                    {
                        _Logger.Warning("UpdateTypes.Weight has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.UpdateTypes.Face:
                    if (e.DataType == MessageHandler.UpdateItemEventArgs.UpdateDataTypes.UInt32)
                    {
                        if (item.Face != (UInt32)e.UpdateValueUInt32)
                        {
                            item.Face = (UInt32)e.UpdateValueUInt32;
                            UpdatedProperties = new string[] { nameof(Item.Face) };
                        }
                    }
                    else
                    {
                        _Logger.Warning("UpdateTypes.Face has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.UpdateTypes.Name:
                    if (e.DataType == MessageHandler.UpdateItemEventArgs.UpdateDataTypes.String)
                    {
                        if ((item.Name != e.UpdateString) || (item.NamePlural != e.UpdateStringPlural))
                        {
                            item.Name = e.UpdateString;
                            item.NamePlural = e.UpdateStringPlural;
                            UpdatedProperties = new string[] { nameof(Item.Name), nameof(Item.NamePlural) };
                        }
                    }
                    else
                    {
                        _Logger.Warning("UpdateTypes.Name has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.UpdateTypes.Animation:
                    if (e.DataType == MessageHandler.UpdateItemEventArgs.UpdateDataTypes.UInt16)
                    {
                        if (item.Animation != e.UpdateValueUInt16)
                        {
                            item.Animation = e.UpdateValueUInt16;
                            UpdatedProperties = new string[] { nameof(Item.Animation) };
                        }
                    }
                    else
                    {
                        _Logger.Warning("UpdateTypes.Animation has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.UpdateTypes.AnimationSpeed:
                    if (e.DataType == MessageHandler.UpdateItemEventArgs.UpdateDataTypes.UInt8)
                    {
                        if (item.AnimationSpeed != e.UpdateValueUInt8)
                        {
                            item.AnimationSpeed = e.UpdateValueUInt8;
                            UpdatedProperties = new string[] { nameof(Item.AnimationSpeed) };
                        }
                    }
                    else
                    {
                        _Logger.Warning("UpdateTypes.AnimationSpeed has wrong datatype: {0}", e.DataType);
                    }
                    break;

                case NewClient.UpdateTypes.NumberOf:
                    if (e.DataType == MessageHandler.UpdateItemEventArgs.UpdateDataTypes.UInt32)
                    {
                        if (item.RawNumberOf != e.UpdateValueUInt32)
                        {
                            item.RawNumberOf = e.UpdateValueUInt32;
                            UpdatedProperties = new string[] { nameof(Item.RawNumberOf) };
                        }
                    }
                    else
                    {
                        _Logger.Warning("UpdateTypes.NumberOf has wrong datatype: {0}", e.DataType);
                    }
                    break;
            }

            if (UpdatedProperties != null)
                OnDataChanged(ModificationTypes.Updated, item, ix, UpdatedProperties);
            else
                _Logger.Debug("Update {0} did not change properties of {1}", e.UpdateType, item);


            //handle container opened/closed 
            if (e.UpdateType == NewClient.UpdateTypes.Flags)
            {
                //item was open, is no longer open
                if ((item == OpenContainer) && !item.IsOpen)
                {
                    OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Closed,
                        item, ix);
                    OpenContainer = null;
                }

                if (item.IsOpen)
                {
                    if (OpenContainer != null)
                    {
                        _Logger.Warning("Container {0} was already open when received updated open container {1}", OpenContainer, item);
    
                        OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Closed,
                            OpenContainer, this.GetIndex(OpenContainer));
                    }

                    OpenContainer = item;
                    OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Opened,
                            item, ix);
                }
            }
        }

        private void _Handler_Player(object sender, MessageHandler.PlayerEventArgs e)
        {
            _PlayerTag = e.tag;

            if (_PlayerTag == 0)
                return;

            for (int ix = 0; ix < this.Count; ix++)
            {
                var item = GetData(ix);

                var newLocation = GetLocation(item.LocationTag, out var newInContainer);

                if ((item.Location != newLocation) || (item.IsInContainer != newInContainer))
                {
                    item.Location = newLocation;
                    item.IsInContainer = newInContainer;

                    OnDataChanged(ModificationTypes.Updated,
                        item, ix, new string[] { nameof(Item.Location), nameof(Item.IsInContainer) });
                }
            }
        }

        private void _Handler_BeginItem2(object sender, EventArgs e)
        {
            _Logger.Debug("Begin add items");

            StartMultiCommand();
        }

        private void _Handler_EndItem2(object sender, EventArgs e)
        {
            _Logger.Debug("End add items");

            EndMultiCommand();
        }

        private void _Handler_BeginUpdateItem(object sender, MessageHandler.BeginEndUpdateItemUpdateEventArgs e)
        {
            if ((_PlayerTag > 0) && (e.ObjectTag == _PlayerTag))
                return;

            _Logger.Debug("Begin update item");

            StartMultiCommand();
        }


        private void _Handler_EndUpdateItem(object sender, MessageHandler.BeginEndUpdateItemUpdateEventArgs e)
        {
            if ((_PlayerTag > 0) && (e.ObjectTag == _PlayerTag))
                return;

            _Logger.Debug("End update item");

            EndMultiCommand();
        }


        private void _Handler_BeginDeleteItem(object sender, EventArgs e)
        {
            _Logger.Debug("Begin delete items");

            StartMultiCommand();
        }

        private void _Handler_EndDeleteItem(object sender, EventArgs e)
        {
            _Logger.Debug("End delete items");

            EndMultiCommand();
        }

        private ItemLocations GetLocation(UInt32 locationTag, out bool inContainer)
        {
            inContainer = false;

            if (locationTag == 0)
                return ItemLocations.Ground;

            if ((_PlayerTag > 0) && (locationTag == _PlayerTag))
                return ItemLocations.Player;

            //Get the container that holds the item
            var container = GetItemByTag(locationTag);

            //No valid container holding the item (or player holding but no player tag)
            if (container == null)
                return ItemLocations.Unknown;

            //we have a valid container at this point
            inContainer = true;

            //have a valid container, need to determine who holds the container
            while (container != null)
            {
                if (container.IsOnGround)
                    return ItemLocations.Ground;

                if ((_PlayerTag > 0) && (container.LocationTag == _PlayerTag))
                    return ItemLocations.Player;

                container = GetItemByTag(container.LocationTag);
            }

            return ItemLocations.Unknown;
        }

        public Item GetItemContainer(Item item)
        {
            if (item == null)
                return null;

            if (item.IsOnGround)
                return null;

            if ((_PlayerTag == 0) || (item.LocationTag == _PlayerTag))
                return null;

            return GetItemByTag(item.LocationTag);
        }

        public bool IsItemContainerOpen(Item item)
        {
            //Technically we can just look at the OpenContainer variable and 
            //check the tags, but this way we future proof for multiple
            //open containers
            var container = GetItemContainer(item);
            if (container == null)
                return false;

            return container.IsOpen;
        }

        public bool IsItemContainerApplied(Item item)
        {
            var container = GetItemContainer(item);
            if (container == null)
                return false;

            return container.IsApplied;
        }

        /// <summary>
        /// Moves item from an open container (ground or player) to the player inventory
        /// Moves item from the ground to the open container (ground or player), then active container (player), then player inventory
        /// </summary>
        public void MoveItemToPlayer(Item item, int count = 0)
        {
            if (item == null)
                return;

            Builder.SendMove((Int32)_PlayerTag, (Int32)item.Tag, count);
        }

        public void MoveItemToGround(Item item, int count = 0)
        {
            if (item == null)
                return;

            Builder.SendMove(0, (Int32)item.Tag, count);
        }

        public void MoveItemToContainer(Item item, Item container, int count = 0)
        {
            if (item == null || container == null)
                return;

            Builder.SendMove((Int32)container.Tag, (Int32)item.Tag, count);
        }

        public void MoveItemToOpenContainer(Item item, int count = 0)
        {
            MoveItemToContainer(item, OpenContainer, count);
        }
    }

    public class ContainerModifiedEventArgs : EventArgs
    {
        public enum ModificationTypes
        {
            Opened,
            Closed
        }

        public ModificationTypes Modification { get; set; }
        public Item Item { get; set; }
        public UInt32 ItemTag => Item.Tag;
        public int ItemIndex { get; set; }

        public override string ToString()
        {
            return string.Format("ContainerModified[{0}]: {1}", Modification, Item);
        }
    }

    public enum ItemLocations
    {
        Unknown,
        Ground,
        Player,
    }

    public class Item
    {
        /// <summary>
        /// Tag of item
        /// </summary>
        public UInt32 Tag { get; set; }

        /// <summary>
        /// Tag of object that owns Item, 0 if on ground
        /// </summary>
        public UInt32 LocationTag { get; set; }
        public NewClient.ItemFlags Flags { get; set; }
        public NewClient.ItemFlags FlagsNoPick => Flags & ~NewClient.ItemFlags.NoPick;


        /// <summary>
        /// Item weight in kg
        /// </summary>
        public float Weight { get; set; }
        public UInt32 Face { get; set; }
        public string Name { get; set; }
        public string NameCase => Name.ToTitleCase();
        public string NamePlural { get; set; }
        public string NamePluralCase => NamePlural.ToTitleCase();
        public UInt16 Animation { get; set; }
        public byte AnimationSpeed { get; set; }

        public UInt32 RawNumberOf { get; set; }

        /// <summary>
        /// Actual number of items, 1 or more
        /// </summary>
        public UInt32 NumberOf => RawNumberOf < 1 ? 1 : RawNumberOf;

        /// <summary>
        /// NumberOf in Words
        /// </summary>
        public string NumberOfInWords => NumberOf < _NumberWords.Length ? _NumberWords[NumberOf] : NumberOf.ToString();

        /// <summary>
        /// NumberOf, but a blank instead of "1"
        /// </summary>
        public string NumberOfWithout1 => NumberOf > 1 ? NumberOf.ToString() : "";

        /// <summary>
        /// NumberOf, but a blank instead of "One"
        /// </summary>
        public string NumberOfInWordsWithout1 => NumberOf > 1 ? NumberOfInWords : "";

        static string[] _NumberWords = new string[] { "None", "One", "Two", "Three", "Four", "Five",
            "Six", "Seven", "Eight", "Nine", "Ten" };

        public UInt16 ClientType { get; set; }

        /* From here, are new item properties and helper functions */

        /// <summary>
        /// Item weight in kg multiplied by NumberOf
        /// </summary>
        public float TotalWeight => !HasWeight ? float.NaN : Weight * NumberOf;

        /// <summary>
        /// Location of item (this does not test if in container or not)
        /// </summary>
        public ItemLocations Location { get; set; }

        /// <summary>
        /// Check if item is in a container (Container may be on the player or on the ground)
        /// </summary>
        public bool IsInContainer { get; set; }

        /// <summary>
        /// Item is on the ground, and ground only (this does not test for in a container on the ground)
        /// </summary>
        public bool IsOnGround => LocationTag == 0;
        public bool HasWeight => !float.IsNaN(Weight);
        public bool IsApplied => (Flags & NewClient.ItemFlags.Applied_Mask) > 0;
        public NewClient.ItemFlags ApplyType => Flags & NewClient.ItemFlags.Applied_Mask;
        public bool IsUnidentified => Flags.HasFlag(NewClient.ItemFlags.Unidentified);
        public bool IsUnpaid => Flags.HasFlag(NewClient.ItemFlags.Unpaid);
        public bool IsMagic => Flags.HasFlag(NewClient.ItemFlags.Magic);
        public bool IsCursed => Flags.HasFlag(NewClient.ItemFlags.Cursed);
        public bool IsDamned => Flags.HasFlag(NewClient.ItemFlags.Damned);
        public bool IsOpen => Flags.HasFlag(NewClient.ItemFlags.Open);
        public bool IsNoPick => Flags.HasFlag(NewClient.ItemFlags.NoPick);
        public bool IsLocked => Flags.HasFlag(NewClient.ItemFlags.Locked);
        public bool IsBlessed => Flags.HasFlag(NewClient.ItemFlags.Blessed);
        public bool IsRead => Flags.HasFlag(NewClient.ItemFlags.Read);

        public override string ToString()
        {
            return string.Format("Item: {0} (Tag={1} Face={2} Flags={3} Location={4} Number={5})",
                Name, Tag, Face, Flags, LocationTag, RawNumberOf);
        }

        public object[] FormatArgs => new object[]
        {
            NameCase,
            NamePluralCase,
            NumberOf,
            NumberOfInWords,
            NumberOfWithout1,
            NumberOfInWordsWithout1,
            Weight,
            TotalWeight,
            Tag,
            LocationTag,
            Flags,
            FlagsNoPick,
            ClientTypes.GetClientTypeInfo(ClientType, out var clientGroup),
        };

        public const string FormatHelp = "{0}=Name {1}=PluralName {2}=NumberOf {3}=NumberOfInWords " +
            "{4}=NumberOfWithout1 {5}=NumberOfInWordsWithout1 {6}=Weight {7}=TotalWeight {8}=Tag " +
            "{9}=Location {10}=Flags {11}=FlagsNoPick {12}=ClientType";
    }
}

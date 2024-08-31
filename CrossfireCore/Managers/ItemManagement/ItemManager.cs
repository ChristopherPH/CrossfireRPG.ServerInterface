using Common;
using CrossfireRPG.ServerInterface.Definitions;
using CrossfireRPG.ServerInterface.Network;
using CrossfireRPG.ServerInterface.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossfireCore.Managers.ItemManagement
{
    public class ItemManager : DataListManager<UInt32, Item, List<Item>>
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
        public override DataModificationTypes SupportedModificationTypes => base.SupportedModificationTypes |
            DataModificationTypes.Added | DataModificationTypes.Updated | DataModificationTypes.Removed |
            DataModificationTypes.MultiCommandStart | DataModificationTypes.MultiCommandEnd;

        private UInt32 _PlayerTag = 0;
        public static Logger Logger { get; } = new Logger(nameof(ItemManager));

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
            return GetDataObject(ItemTag);
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

        protected override void ClearData(bool disconnected)
        {
            if (OpenContainer != null)
            {
                OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Closed,
                    OpenContainer, this.GetDataObjectIndex(OpenContainer));

                OpenContainer = null;
            }

            //Log what items are being cleared. Technically the server should be deleting
            //all player items between characters, but it doesn't seem to get all of them
            if (this.DataObjectCount > 0)
            {
                Logger.Debug("Begin clear ItemManager");

                foreach (var item in this)
                    Logger.Debug("Clearing {0}", item);

                Logger.Debug("End clear ItemManager");
            }

            base.ClearData(disconnected);
        }

        private void _Handler_Item2(object sender, MessageHandler.Item2EventArgs e)
        {
            //If the item already exists, then update it instead.
            //Note: This happens when picking up items from the ground (locationTag differs)
            if (GetDataObject(e.item_tag, out var item, out var index))
            {
                var oldLocationTag = item.LocationTag;

                item.BeginPropertiesChanged();

                //no need to update item.Tag
                item.LocationTag = e.item_location;
                item.Flags = (NewClient.ItemFlags)e.item_flags;
                item.Weight = e.item_weight;
                item.Face = e.item_face;
                item.Name = e.item_name;
                item.NamePlural = e.item_name_plural;
                item.Animation = e.item_anim;
                item.AnimationSpeed = e.item_animspeed;
                item.RawNumberOf = e.item_nrof;
                item.ClientType = e.item_type;

                item.Location = GetLocation(item.LocationTag, out var inContainer);
                item.IsInContainer = inContainer;

                var updatedProperties = item.EndPropertiesChanged();

                if (updatedProperties == null)
                {
                    Logger.Warning("Trying to add existing object {0}, updating instead: (no properties changed)", e.item_tag);
                }
                else if ((oldLocationTag != item.LocationTag) && (oldLocationTag == 0))
                {
                    Logger.Debug("Trying to add existing object {0}, updating instead: {1} (picked up item from ground)", e.item_tag,
                        string.Join(", ", updatedProperties));
                }
                else
                {
                    Logger.Warning("Trying to add existing object {0}, updating instead: {1}", e.item_tag,
                        string.Join(", ", updatedProperties));
                }

                UpdateDataObject(e.item_tag, updatedProperties);
            }
            else
            {
                item = new Item()
                {
                    Tag = e.item_tag,
                    LocationTag = e.item_location,
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

                index = AddDataObject(e.item_tag, item);
            }

            Logger.Info("Added {0}", item);

            if (item.IsOpen && (item != OpenContainer)) //avoid issue with picking up an open container
            {
                if (OpenContainer != null)
                {
                    Logger.Warning("Container {0} was already open when received new open container {1}", OpenContainer, item);

                    OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Closed,
                        OpenContainer, this.GetDataObjectIndex(OpenContainer));
                }

                OpenContainer = item;
                OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Opened,
                        item, index);
            }
        }

        private void _Handler_DeleteItem(object sender, MessageHandler.DeleteItemEventArgs e)
        {
            if (!GetDataObject(e.ObjectTag, out var item, out var index))
            {
                //We will get these messages when applying a bed to reality, when we have
                //applied containers with items inside, that we haven't opened in this session.
                //The server has not sent the items to the client, but sends a removal message
                //when quitting.
                //We will also get these messages when deleting a character, as the player list
                //is sent before the deletion of inventory. The item manager will be cleared on
                //a seeing the player list.
                Logger.Warning("Trying to delete invalid object {0}", e.ObjectTag);
                return;
            }

            if (item == OpenContainer)
            {
                OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Closed,
                    item, index);
                OpenContainer = null;
            }

            Logger.Info("Deleted {0}", item);

            this.RemoveDataObject(e.ObjectTag);
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

            Logger.Info("Deleting inventory of {0}", location);

            var items = this.Where(x => x.LocationTag == (uint)e.ObjectTag).ToList();
            if (items.Count > 0)
            {
                Logger.Debug("Begin delete inventory");
                this.StartMultiCommand();

                foreach (var item in items)
                {
                    Logger.Debug("Deleted {0} from {1}", item, location);

                    if (item == OpenContainer)
                    {
                        OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Closed,
                            item, this.GetDataObjectIndex(item));
                        OpenContainer = null;
                    }

                    this.RemoveDataObject(item.Tag);
                }

                Logger.Debug("End delete inventory");
                this.EndMultiCommand();
            }
        }

        private void _Handler_UpdateItem(object sender, MessageHandler.UpdateItemEventArgs e)
        {
           if ((_PlayerTag > 0) && (e.ObjectTag == _PlayerTag))
                return;

            if (!GetDataObject(e.ObjectTag, out var item, out var index))
            {
                Logger.Warning("Trying to update invalid object {0}", e.ObjectTag);
                return;
            }

            Logger.Info("Update {0} of {1} to ({2}/{3}/{4}/{5}) {6} {7}",
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
                        Logger.Warning("UpdateTypes.Location has wrong datatype: {0}", e.DataType);
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
                        Logger.Warning("UpdateTypes.Flags has wrong datatype: {0}", e.DataType);
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
                        Logger.Warning("UpdateTypes.Weight has wrong datatype: {0}", e.DataType);
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
                        Logger.Warning("UpdateTypes.Face has wrong datatype: {0}", e.DataType);
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
                        Logger.Warning("UpdateTypes.Name has wrong datatype: {0}", e.DataType);
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
                        Logger.Warning("UpdateTypes.Animation has wrong datatype: {0}", e.DataType);
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
                        Logger.Warning("UpdateTypes.AnimationSpeed has wrong datatype: {0}", e.DataType);
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
                        Logger.Warning("UpdateTypes.NumberOf has wrong datatype: {0}", e.DataType);
                    }
                    break;
            }

            if (UpdatedProperties != null)
                OnDataChanged(DataModificationTypes.Updated, item, index, UpdatedProperties);
            else
                Logger.Debug("Update {0} did not change properties of {1}", e.UpdateType, item);


            //handle container opened/closed 
            if (e.UpdateType == NewClient.UpdateTypes.Flags)
            {
                //item was open, is no longer open
                if ((item == OpenContainer) && !item.IsOpen)
                {
                    OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Closed,
                        item, index);
                    OpenContainer = null;
                }

                if (item.IsOpen)
                {
                    if (OpenContainer != null)
                    {
                        Logger.Warning("Container {0} was already open when received updated open container {1}", OpenContainer, item);
    
                        OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Closed,
                            OpenContainer, this.GetDataObjectIndex(OpenContainer));
                    }

                    OpenContainer = item;
                    OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Opened,
                            item, index);
                }
            }
        }

        private void _Handler_Player(object sender, MessageHandler.PlayerEventArgs e)
        {
            _PlayerTag = e.tag;

            if (_PlayerTag == 0)
                return;

            for (int ix = 0; ix < this.DataObjectCount; ix++)
            {
                var item = GetDataObjectByIndex(ix);

                var newLocation = GetLocation(item.LocationTag, out var newInContainer);

                if ((item.Location != newLocation) || (item.IsInContainer != newInContainer))
                {
                    item.Location = newLocation;
                    item.IsInContainer = newInContainer;

                    OnDataChanged(DataModificationTypes.Updated,
                        item, ix, new string[] { nameof(Item.Location), nameof(Item.IsInContainer) });
                }
            }
        }

        private void _Handler_BeginItem2(object sender, EventArgs e)
        {
            Logger.Debug("Begin add items");

            StartMultiCommand();
        }

        private void _Handler_EndItem2(object sender, EventArgs e)
        {
            Logger.Debug("End add items");

            EndMultiCommand();
        }

        private void _Handler_BeginUpdateItem(object sender, MessageHandler.BeginEndUpdateItemUpdateEventArgs e)
        {
            if ((_PlayerTag > 0) && (e.ObjectTag == _PlayerTag))
                return;

            Logger.Debug("Begin update item");

            StartMultiCommand();
        }


        private void _Handler_EndUpdateItem(object sender, MessageHandler.BeginEndUpdateItemUpdateEventArgs e)
        {
            if ((_PlayerTag > 0) && (e.ObjectTag == _PlayerTag))
                return;

            Logger.Debug("End update item");

            EndMultiCommand();
        }


        private void _Handler_BeginDeleteItem(object sender, EventArgs e)
        {
            Logger.Debug("Begin delete items");

            StartMultiCommand();
        }

        private void _Handler_EndDeleteItem(object sender, EventArgs e)
        {
            Logger.Debug("End delete items");

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

            //item is on ground, therefore not in a container
            if (item.IsOnGround)
                return null;

            //item is on player, therefore not in a container
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

            Builder.SendProtocolMove((Int32)_PlayerTag, (Int32)item.Tag, count);
        }

        public void MoveItemToGround(Item item, int count = 0)
        {
            if (item == null)
                return;

            Builder.SendProtocolMove(0, (Int32)item.Tag, count);
        }

        public void MoveItemToContainer(Item item, Item container, int count = 0)
        {
            if (item == null || container == null)
                return;

            Builder.SendProtocolMove((Int32)container.Tag, (Int32)item.Tag, count);
        }

        public void MoveItemToOpenContainer(Item item, int count = 0)
        {
            MoveItemToContainer(item, OpenContainer, count);
        }
    }

    public enum ItemLocations
    {
        Unknown,
        Ground,
        Player,
    }
}

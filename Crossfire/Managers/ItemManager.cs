using Common;
using CrossfireCore;
using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crossfire.Managers
{
    public class ItemManager
    {
        public ItemManager(SocketConnection Connection, MessageBuilder Builder, MessageParser Parser)
        {
            _Connection = Connection;
            _Builder = Builder;
            _Parser = Parser;

            _Connection.OnStatusChanged += _Connection_OnStatusChanged;
            _Parser.Item2 += _Parser_Item2;
            _Parser.UpdateItem += _Parser_UpdateItem;
            _Parser.DeleteItem += _Parser_DeleteItem;
            _Parser.DeleteInventory += _Parser_DeleteInventory;
            _Parser.Player += _Parser_Player;
            _Parser.BeginItem2 += _Parser_BeginItem2;
            _Parser.EndItem2 += _Parser_EndItem2;
            _Parser.BeginDeleteItem += _Parser_BeginDeleteItem;
            _Parser.EndDeleteItem += _Parser_EndDeleteItem;
            _Parser.BeginUpdateItem += _Parser_BeginUpdateItem;
            _Parser.EndUpdateItem += _Parser_EndUpdateItem;
        }

        private SocketConnection _Connection;
        private MessageBuilder _Builder;
        private MessageParser _Parser;
        private UInt32 _PlayerTag = 0;
        static Logger _Logger = new Logger(nameof(ItemManager));

        public List<Item> Items { get; } = new List<Item>();
        public Item OpenContainer { get; private set; } = null;

        public IEnumerable<Item> PlayerItems => Items.Where(x => x.Location == ItemLocations.Player);
        public IEnumerable<Item> PlayerHeldItems => PlayerItems.Where(x => !x.IsInContainer);
        public IEnumerable<Item> PlayerContainedItems => PlayerItems.Where(x => x.IsInContainer);
        public IEnumerable<Item> PlayerActiveContainers => PlayerItems.Where(x => x.ApplyType == NewClient.ItemFlags.Active);
        public IEnumerable<Item> GroundItems => Items.Where(x => x.IsOnGround);
        public IEnumerable<Item> ContainedItems => Items.Where(x => x.IsInContainer);
        public IEnumerable<Item> OpenContainerItems => (OpenContainer == null) ? Enumerable.Empty<Item>() :
            ContainedItems.Where(x => x.LocationTag == OpenContainer.Tag);

        public Item GetItemByTag(UInt32 ItemTag)
        {
            return Items.FirstOrDefault(x => x.Tag == ItemTag);
        }

        public event EventHandler<ItemModifiedEventArgs> ItemChanged;
        public event EventHandler<ContainerModifiedEventArgs> ContainerChanged;

        protected void OnItemChanged(ItemModifiedEventArgs.ModificationTypes ChangeType, NewClient.UpdateTypes UpdateType, Item Item, int ItemIndex)
        {
            ItemChanged?.Invoke(this, new ItemModifiedEventArgs()
            {
                Modification = ChangeType,
                UpdateType = UpdateType,
                Item = Item,
                ItemIndex = ItemIndex
            });
        }

        protected void OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes ChangeType, Item Item, int ItemIndex)
        {
            ContainerChanged?.Invoke(this, new ContainerModifiedEventArgs()
            {
                Modification = ChangeType,
                Item = Item,
                ItemIndex = ItemIndex
            });
        }

        private void _Connection_OnStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            if (OpenContainer != null)
            {
                OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Closed,
                    OpenContainer, Items.IndexOf(OpenContainer));

                OpenContainer = null;
            }

            //TODO: do we need to send ChangedEvents when this happens?
            Items.Clear();
        }

        private void _Parser_Item2(object sender, MessageParser.Item2EventArgs e)
        {
            var item = new Item()
            {
                LocationTag = e.item_location,
                Tag = e.item_tag,
                Flags = (NewClient.ItemFlags)e.item_flags,
                Weight = e.item_weight == uint.MaxValue ? float.NaN : (float)e.item_weight / 1000,
                Face = e.item_face,
                Name = e.item_name,
                NamePlural = e.item_name_plural,
                Animation = e.item_anim,
                AnimationSpeed = e.item_animspeed,
                NumberOf = e.item_nrof,
                ClientType = e.item_type,
            };

            item.Location = GetLocation(item.LocationTag, out var inContainer);
            item.IsInContainer = inContainer;

            var ix = Items.FindIndex(x => x.Tag == e.item_tag);
            if (ix != -1)
            {
                _Logger.Warning("Trying to add existing object {0}, updating instead", e.item_tag);

                Items[ix] = item;
                OnItemChanged(ItemModifiedEventArgs.ModificationTypes.Updated, 
                    NewClient.UpdateTypes.All, item, ix);
            }
            else
            {
                Items.Add(item);
                ix = Items.Count - 1;

                OnItemChanged(ItemModifiedEventArgs.ModificationTypes.Added, 
                    NewClient.UpdateTypes.All, item, ix);
            }

            _Logger.Info("Added {0}", item);

            if (item.IsOpen)
            {
                if (OpenContainer != null)
                {
                    _Logger.Warning("Container {0} was already open when received new open container {1}", OpenContainer, item);

                    OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Closed,
                        OpenContainer, Items.IndexOf(OpenContainer));
                }

                OpenContainer = item;
                OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Opened,
                        item, ix);
            }
        }

        private void _Parser_DeleteItem(object sender, MessageParser.DeleteItemEventArgs e)
        {
            var ix = Items.FindIndex(x => x.Tag == e.ObjectTag);
            if (ix == -1)
            {
                _Logger.Warning("Trying to delete invalid object {0}", e.ObjectTag);
                return;
            }

            var item = Items[ix];


            if (item == OpenContainer)
            {
                OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Closed,
                    item, ix);
                OpenContainer = null;
            }

            _Logger.Info("Deleted {0}", item);

            //trigger event before actually removing item, so listeners can view item data
            OnItemChanged(ItemModifiedEventArgs.ModificationTypes.Removed, 
                NewClient.UpdateTypes.All, item, ix);

            Items.RemoveAt(ix);
        }

        private void _Parser_DeleteInventory(object sender, MessageParser.DeleteInventoryEventArgs e)
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

            var items = Items.Where(x => x.LocationTag == (uint)e.ObjectTag).ToList();
            if (items.Count > 0)
            {
                OnItemChanged(ItemModifiedEventArgs.ModificationTypes.BatchStart,
                    0, null, -1);

                foreach (var item in items)
                {
                    _Logger.Debug("Deleted {0} from {1}", item, location);

                    if (item == OpenContainer)
                    {
                        OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Closed,
                            item, Items.IndexOf(item));
                        OpenContainer = null;
                    }

                    //trigger event before actually removing item, so listeners can view item data
                    OnItemChanged(ItemModifiedEventArgs.ModificationTypes.Removed, 
                        NewClient.UpdateTypes.All, item, Items.IndexOf(item));

                    Items.Remove(item);
                }

                OnItemChanged(ItemModifiedEventArgs.ModificationTypes.BatchEnd,
                    0, null, -1);
            }
        }

        private void _Parser_UpdateItem(object sender, MessageParser.UpdateItemEventArgs e)
        {
           if ((_PlayerTag > 0) && (e.ObjectTag == _PlayerTag))
                return;

            var ix = Items.FindIndex(x => x.Tag == e.ObjectTag);
            if (ix == -1)
            {
                _Logger.Warning("Trying to update invalid object {0}", e.ObjectTag);
                return;
            }

            var item = Items[ix];

            _Logger.Info("Update {0} of {1} to {2}/{3} {4}",
                e.UpdateType, item, e.UpdateValue, e.UpdateString, e.UpdateStringPlural);

            switch (e.UpdateType)
            {
                case NewClient.UpdateTypes.Location:
                    item.LocationTag = (UInt32)e.UpdateValue;

                    item.Location = GetLocation(item.LocationTag, out var inContainer);
                    item.IsInContainer = inContainer;
                    break;

                case NewClient.UpdateTypes.Flags:
                    item.Flags = (NewClient.ItemFlags)e.UpdateValue;
                    break;

                case NewClient.UpdateTypes.Weight:
                    if (e.UpdateValue == uint.MaxValue)
                        item.Weight = float.NaN;
                    else
                        item.Weight = (float)e.UpdateValue / 1000;
                    break;

                case NewClient.UpdateTypes.Face:
                    item.Face = (UInt32)e.UpdateValue;
                    break;

                case NewClient.UpdateTypes.Name:
                    item.Name = e.UpdateString;
                    item.NamePlural = e.UpdateStringPlural;
                    break;

                case NewClient.UpdateTypes.Animation:
                    item.Animation = (UInt16)e.UpdateValue;
                    break;

                case NewClient.UpdateTypes.AnimationSpeed:
                    item.AnimationSpeed = (byte)e.UpdateValue;
                    break;

                case NewClient.UpdateTypes.NumberOf:
                    item.NumberOf = (UInt32)e.UpdateValue;
                    break;
            }

            OnItemChanged(ItemModifiedEventArgs.ModificationTypes.Updated, e.UpdateType, item, ix);

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
                            OpenContainer, Items.IndexOf(OpenContainer));
                    }

                    OpenContainer = item;
                    OnContainerChanged(ContainerModifiedEventArgs.ModificationTypes.Opened,
                            item, ix);
                }
            }
        }

        private void _Parser_Player(object sender, MessageParser.PlayerEventArgs e)
        {
            _PlayerTag = e.tag;

            if (_PlayerTag == 0)
            {
                Items.Clear();
                OpenContainer = null;
                return;
            }

            for (int ix = 0; ix < Items.Count; ix++)
            {
                var item = Items[ix];

                var newLocation = GetLocation(item.LocationTag, out var newInContainer);

                if ((item.Location != newLocation) || (item.IsInContainer != newInContainer))
                {
                    item.Location = newLocation;
                    item.IsInContainer = newInContainer;

                    OnItemChanged(ItemModifiedEventArgs.ModificationTypes.Updated, 
                        NewClient.UpdateTypes.Location, item, ix);
                }
            }
        }

        private void _Parser_BeginItem2(object sender, EventArgs e)
        {
            _Logger.Debug("Begin add items");

            OnItemChanged(ItemModifiedEventArgs.ModificationTypes.BatchStart,
               0, null, -1);
        }

        private void _Parser_EndItem2(object sender, EventArgs e)
        {
            _Logger.Debug("End add items");

            OnItemChanged(ItemModifiedEventArgs.ModificationTypes.BatchEnd,
                0, null, -1);
        }

        private void _Parser_BeginUpdateItem(object sender, MessageParser.UpdateItemEventArgs e)
        {
            if ((_PlayerTag > 0) && (e.ObjectTag == _PlayerTag))
                return;

            _Logger.Debug("Begin update item");

            OnItemChanged(ItemModifiedEventArgs.ModificationTypes.BatchStart,
                0, null, -1);
        }


        private void _Parser_EndUpdateItem(object sender, MessageParser.UpdateItemEventArgs e)
        {
            if ((_PlayerTag > 0) && (e.ObjectTag == _PlayerTag))
                return;

            _Logger.Debug("End update item");

            OnItemChanged(ItemModifiedEventArgs.ModificationTypes.BatchEnd,
                0, null, -1);
        }


        private void _Parser_BeginDeleteItem(object sender, EventArgs e)
        {
            _Logger.Debug("Begin delete items");

            OnItemChanged(ItemModifiedEventArgs.ModificationTypes.BatchStart,
                0, null, -1);
        }

        private void _Parser_EndDeleteItem(object sender, EventArgs e)
        {
            _Logger.Debug("End delete items");

            OnItemChanged(ItemModifiedEventArgs.ModificationTypes.BatchEnd,
                0, null, -1);
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
        /// Moves item to player's open container, then active container, then player inventory
        /// </summary>
        /// <param name="item"></param>
        /// <param name="count"></param>
        public void MoveItemToPlayer(Item item, int count = 0)
        {
            if (item == null)
                return;

            _Builder.SendMove(_PlayerTag.ToString(), item.Tag.ToString(), count.ToString());
        }

        public void MoveItemToGround(Item item, int count = 0)
        {
            if (item == null)
                return;

            _Builder.SendMove("0", item.Tag.ToString(), count.ToString());
        }

        public void MoveItemToContainer(Item item, Item container, int count = 0)
        {
            if (item == null || container == null)
                return;

            _Builder.SendMove(container.Tag.ToString(), item.Tag.ToString(), count.ToString());
        }
    }

    public class ItemModifiedEventArgs : EventArgs
    {
        public enum ModificationTypes
        {
            Added,
            Removed,
            Updated,
            BatchStart,
            BatchEnd
        }

        public ModificationTypes Modification { get; set; }
        public NewClient.UpdateTypes UpdateType { get; set; } = 0;
        public Item Item { get; set; }
        public UInt32 ItemTag => Item.Tag;
        public int ItemIndex { get; set; }

        public override string ToString()
        {
            return string.Format("ItemModified[{0}]: {1}", Modification, Item);
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

        /// <summary>
        /// Item weight in kg
        /// </summary>
        public float Weight { get; set; }

        public UInt32 Face { get; set; }
        public string Name { get; set; }
        public string NamePlural { get; set; }
        public UInt16 Animation { get; set; }
        public byte AnimationSpeed { get; set; }
        public UInt32 NumberOf { get; set; }
        public UInt16 ClientType { get; set; }

        /* From here, are new item properties and helper functions */

        /// <summary>
        /// Item weight in kg multiplied by NumberOf
        /// </summary>
        public float TotalWeight => !HasWeight ? float.NaN : Weight * (NumberOf < 1 ? 1 : NumberOf);

        public ItemLocations Location { get; set; }
        public bool IsInContainer { get; set; }


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
                Name, Tag, Face, Flags, LocationTag, NumberOf);
        }
    }
}

using CrossfireCore;
using CrossfireCore.ServerInterface;
using CrossfireCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Crossfire
{
    public class ItemManager
    {
        public ItemManager(MessageParser Parser)
        {
            _Parser = Parser;

            _Parser.Item2 += _Parser_Item2;
            _Parser.UpdateItem += _Parser_UpdateItem;
            _Parser.DeleteItem += _Parser_DeleteItem;
            _Parser.DeleteInventory += _Parser_DeleteInventory;
            _Parser.Player += _Parser_Player;
        }

        private MessageParser _Parser;
        private UInt32 _PlayerTag = 0;
        static Logger _Logger = new Logger(nameof(ItemManager));

        public List<Item> Items { get; } = new List<Item>();

        public IEnumerable<Item> PlayerItems =>
            Items.Where(x => (x.Location == _PlayerTag) && (_PlayerTag != 0));
        public IEnumerable<Item> GroundItems =>
            Items.Where(x => x.Location == 0);
        public IEnumerable<Item> ContainedItems =>
            Items.Where(x => (x.Location != 0) && ((_PlayerTag != 0) && (x.Location != _PlayerTag)));

        public Item GetItemByTag(UInt32 ItemTag)
        {
            return Items.FirstOrDefault(x => x.Tag == ItemTag);
        }

        public event EventHandler<ItemChangedEventArgs> ItemChanged;

        protected void OnItemChanged(ItemChangedEventArgs.ChangeTypes ChangeType, Item Item, int ItemIndex)
        {
            ItemChanged?.Invoke(this, new ItemChangedEventArgs()
            {
                ChangeType = ChangeType,
                Item = Item,
                ItemIndex = ItemIndex
            });
        }

        private void _Parser_Item2(object sender, MessageParserBase.Item2EventArgs e)
        {
            var item = new Item()
            {
                Location = e.item_location,
                Tag = e.item_tag,
                Flags = (NewClient.ItemFlags)e.item_flags,
                Weight = (float)e.item_weight / 1000,
                Face = e.item_face,
                Name = e.item_name,
                NamePlural = e.item_name_plural,
                Animation = e.item_anim,
                AnimationSpeed = e.item_animspeed,
                NumberOf = e.item_nrof,
                ClientType = e.item_type,
            };

            var ix = Items.FindIndex(x => x.Tag == e.item_tag);
            if (ix != -1)
            {
                _Logger.Warning("Trying to add existing object {0}, updating instead", e.item_tag);

                Items[ix] = item;
                OnItemChanged(ItemChangedEventArgs.ChangeTypes.Updated, item, ix);
            }
            else
            {
                Items.Add(item);
                OnItemChanged(ItemChangedEventArgs.ChangeTypes.Added, item, Items.Count - 1);
            }
        }

        private void _Parser_DeleteItem(object sender, MessageParserBase.DeleteItemEventArgs e)
        {
            var ix = Items.FindIndex(x => x.Tag == e.ObjectTag);
            if (ix == -1)
            {
                _Logger.Warning("Trying to delete invalid object {0}", e.ObjectTag);
                return;
            }

            //trigger event before actually removing item, so listeners can view item data
            OnItemChanged(ItemChangedEventArgs.ChangeTypes.Removed, Items[ix], ix);

            Items.RemoveAt(ix);
        }

        private void _Parser_DeleteInventory(object sender, MessageParserBase.DeleteInventoryEventArgs e)
        {
            var items = Items.Where(x => x.Location == (uint)e.ObjectTag).ToList();
            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    //trigger event before actually removing item, so listeners can view item data
                    OnItemChanged(ItemChangedEventArgs.ChangeTypes.Removed, item, Items.IndexOf(item));

                    Items.Remove(item);
                }
            }
        }

        private void _Parser_UpdateItem(object sender, MessageParserBase.UpdateItemEventArgs e)
        {
            var ix = Items.FindIndex(x => x.Tag == e.ObjectTag);
            if (ix == -1)
            {
                _Logger.Warning("Trying to update invalid object {0}", e.ObjectTag);
                return;
            }

            var item = Items[ix];
            var changeType = ItemChangedEventArgs.ChangeTypes.Updated;

            switch (e.UpdateType)
            {
                case NewClient.UpdateTypes.Location:
                    item.Location = (UInt32)e.UpdateValue;
                    break;

                case NewClient.UpdateTypes.Flags:
                    item.Flags = (NewClient.ItemFlags)e.UpdateValue;
                    changeType = ItemChangedEventArgs.ChangeTypes.Flags;
                    break;

                case NewClient.UpdateTypes.Weight:
                    item.Weight = (float)e.UpdateValue / 1000;
                    break;

                case NewClient.UpdateTypes.Face:
                    item.Face = (UInt32)e.UpdateValue;
                    break;

                case NewClient.UpdateTypes.Name:
                    item.Name = e.UpdateString;
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

            OnItemChanged(changeType, item, ix);
        }

        private void _Parser_Player(object sender, MessageParserBase.PlayerEventArgs e)
        {
            _PlayerTag = e.tag;
        }
    }

    public class ItemChangedEventArgs : EventArgs
    {
        public enum ChangeTypes
        {
            Added,
            Removed,
            Updated,
            Flags,
        }

        public ChangeTypes ChangeType { get; set; }
        public Item Item { get; set; }
        public UInt32 ItemTag => Item.Tag;
        public int ItemIndex { get; set; }

        public override string ToString()
        {
            return string.Format("ItemChanged[{0}]: {1}", ChangeType, Item);
        }
    }

    public class Item
    {
        public UInt32 Tag { get; set; }

        /// <summary>
        /// Tag of object that owns Item, 0 if on ground
        /// </summary>
        public UInt32 Location { get; set; }
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


        public bool IsOnGround => Location == 0;
        public bool IsApplied => (Flags & NewClient.ItemFlags.Applied_Mask) > 0;
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
            return string.Format("Item: {0} ({1})", Name, Tag);
        }
    }
}

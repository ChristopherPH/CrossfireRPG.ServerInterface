using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParser
    {
        public event EventHandler<Item2EventArgs> Item2;
        public event EventHandler<EventArgs> BeginItem2;
        public event EventHandler<EventArgs> EndItem2;
        public event EventHandler<UpdateItemEventArgs> UpdateItem;
        public event EventHandler<EventArgs> BeginUpdateItem;
        public event EventHandler<EventArgs> EndUpdateItem;
        public event EventHandler<DeleteItemEventArgs> DeleteItem;
        public event EventHandler<EventArgs> BeginDeleteItem;
        public event EventHandler<EventArgs> EndDeleteItem;
        public event EventHandler<DeleteInventoryEventArgs> DeleteInventory;

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

        protected override void HandleBeginItem2()
        {
            BeginItem2?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandleEndItem2()
        {
            EndItem2?.Invoke(this, EventArgs.Empty);
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

        protected override void HandleUpdateItem(UInt32 ObjectTag, NewClient.UpdateTypes UpdateType,
            string UpdateValue, string UpdateValuePlural)
        {
            UpdateItem?.Invoke(this, new UpdateItemEventArgs()
            {
                ObjectTag = ObjectTag,
                UpdateType = UpdateType,
                UpdateString = UpdateValue,
                UpdateStringPlural = UpdateValuePlural,
            });
        }

        protected override void HandleBeginUpdateItem()
        {
            BeginUpdateItem?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandleEndUpdateItem()
        {
            EndUpdateItem?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandleDeleteItem(uint ObjectTag)
        {
            DeleteItem?.Invoke(this, new DeleteItemEventArgs()
            {
                ObjectTag = ObjectTag
            });
        }

        protected override void HandleBeginDeleteItem()
        {
            BeginDeleteItem?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandleEndDeleteItem()
        {
            EndDeleteItem?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandleDeleteInventory(int ObjectTag)
        {
            DeleteInventory?.Invoke(this, new DeleteInventoryEventArgs()
            {
                ObjectTag = ObjectTag
            });
        }

        public class Item2EventArgs : MultiCommandEventArgs
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

        public class UpdateItemEventArgs : MultiCommandEventArgs
        {
            public UInt32 ObjectTag { get; set; }
            public NewClient.UpdateTypes UpdateType { get; set; }
            public Int64 UpdateValue { get; set; }
            public string UpdateString { get; set; }
            public string UpdateStringPlural { get; set; }
        }

        public class DeleteItemEventArgs : MultiCommandEventArgs
        {
            public UInt32 ObjectTag { get; set; }
        }

        public class DeleteInventoryEventArgs : SingleCommandEventArgs
        {
            public int ObjectTag { get; set; }
        }
    }
}

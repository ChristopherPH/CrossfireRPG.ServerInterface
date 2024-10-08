﻿/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using CrossfireRPG.ServerInterface.Definitions;
using System;
using System.Collections.Generic;

namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageHandler
    {
        public event EventHandler<Item2EventArgs> Item2;
        public event EventHandler<BeginBatchEventArgs> BeginItem2;
        public event EventHandler<EndBatchEventArgs> EndItem2;
        public event EventHandler<UpdateItemEventArgs> UpdateItem;
        public event EventHandler<BeginEndUpdateItemUpdateEventArgs> BeginUpdateItem;
        public event EventHandler<BeginEndUpdateItemUpdateEventArgs> EndUpdateItem;
        public event EventHandler<DeleteItemEventArgs> DeleteItem;
        public event EventHandler<BeginBatchEventArgs> BeginDeleteItem;
        public event EventHandler<EndBatchEventArgs> EndDeleteItem;
        public event EventHandler<DeleteInventoryEventArgs> DeleteInventory;

        private Queue<Item2EventArgs> _Item2BatchEvents = new Queue<Item2EventArgs>();
        private Queue<DeleteItemEventArgs> _DeleteItemBatchEvents = new Queue<DeleteItemEventArgs>();

        protected override void HandleItem2(uint item_location, uint item_tag, uint item_flags,
            float item_weight, uint item_face, string item_name, string item_name_plural,
            ushort item_anim, byte item_animspeed, uint item_nrof, ushort item_type)
        {
            _Item2BatchEvents.Enqueue(new Item2EventArgs()
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
            _Item2BatchEvents.Clear();
        }

        protected override void HandleEndItem2()
        {
            var BatchNumber = 1;
            var BatchCount = _Item2BatchEvents.Count;

            BeginItem2?.Invoke(this, new BeginBatchEventArgs() { BatchCount = BatchCount });

            while (_Item2BatchEvents.Count > 0)
            {
                var args = _Item2BatchEvents.Dequeue();
                args.BatchNumber = BatchNumber++;
                args.BatchCount = BatchCount;
                Item2?.Invoke(this, args);
            }

            _Item2BatchEvents.Clear();

            EndItem2?.Invoke(this, new EndBatchEventArgs() { BatchCount = BatchNumber });
        }

        protected override void HandleUpdateItem(UInt32 ObjectTag, NewClient.UpdateTypes UpdateType, byte UpdateValue)
        {
            UpdateItem?.Invoke(this, new UpdateItemEventArgs(ObjectTag, UpdateType, UpdateValue));
        }

        protected override void HandleUpdateItem(UInt32 ObjectTag, NewClient.UpdateTypes UpdateType, UInt16 UpdateValue)
        {
            UpdateItem?.Invoke(this, new UpdateItemEventArgs(ObjectTag, UpdateType, UpdateValue));
        }

        protected override void HandleUpdateItem(UInt32 ObjectTag, NewClient.UpdateTypes UpdateType, UInt32 UpdateValue)
        {
            UpdateItem?.Invoke(this, new UpdateItemEventArgs(ObjectTag, UpdateType, UpdateValue));
        }

        protected override void HandleUpdateItem(UInt32 ObjectTag, NewClient.UpdateTypes UpdateType, float UpdateValue)
        {
            UpdateItem?.Invoke(this, new UpdateItemEventArgs(ObjectTag, UpdateType, UpdateValue));
        }

        protected override void HandleUpdateItem(UInt32 ObjectTag, NewClient.UpdateTypes UpdateType,
            string UpdateValue, string UpdateValuePlural)
        {
            UpdateItem?.Invoke(this, new UpdateItemEventArgs(ObjectTag, UpdateType, UpdateValue, UpdateValuePlural));
        }

        protected override void HandleBeginUpdateItem(uint ObjectTag)
        {
            BeginUpdateItem?.Invoke(this, new BeginEndUpdateItemUpdateEventArgs()
            {
                ObjectTag = ObjectTag,
            });
        }

        protected override void HandleEndUpdateItem(uint ObjectTag)
        {
            EndUpdateItem?.Invoke(this, new BeginEndUpdateItemUpdateEventArgs()
            {
                ObjectTag = ObjectTag,
            });
        }

        protected override void HandleDeleteItem(uint ObjectTag)
        {
            _DeleteItemBatchEvents.Enqueue(new DeleteItemEventArgs()
            {
                ObjectTag = ObjectTag
            });
        }

        protected override void HandleBeginDeleteItem()
        {
            _DeleteItemBatchEvents.Clear();
        }

        protected override void HandleEndDeleteItem()
        {
            var BatchNumber = 1;
            var BatchCount = _DeleteItemBatchEvents.Count;

            BeginDeleteItem?.Invoke(this, new BeginBatchEventArgs() { BatchCount = BatchCount });

            while (_DeleteItemBatchEvents.Count > 0)
            {
                var args = _DeleteItemBatchEvents.Dequeue();
                args.BatchNumber = BatchNumber++;
                args.BatchCount = BatchCount;
                DeleteItem?.Invoke(this, args);
            }

            _DeleteItemBatchEvents.Clear();

            EndDeleteItem?.Invoke(this, new EndBatchEventArgs() { BatchCount = BatchNumber });
        }

        protected override void HandleDeleteInventory(int ObjectTag)
        {
            DeleteInventory?.Invoke(this, new DeleteInventoryEventArgs()
            {
                ObjectTag = ObjectTag
            });
        }

        public class Item2EventArgs : BatchEventArgs
        {
            public UInt32 item_location { get; set; }
            public UInt32 item_tag { get; set; }
            public UInt32 item_flags { get; set; }
            public float item_weight { get; set; }
            public UInt32 item_face { get; set; }
            public string item_name { get; set; }
            public string item_name_plural { get; set; }
            public UInt16 item_anim { get; set; }
            public byte item_animspeed { get; set; }
            public UInt32 item_nrof { get; set; }
            public UInt16 item_type { get; set; }
        }

        public class UpdateItemEventArgs : BatchEventArgs
        {
            public UpdateItemEventArgs(UInt32 ObjectTag, NewClient.UpdateTypes UpdateType, byte Value)
            {
                this.ObjectTag = ObjectTag;
                this.UpdateType = UpdateType;
                this.DataType = UpdateDataTypes.UInt8;
                this.UpdateValueUInt8 = Value;
            }

            public UpdateItemEventArgs(UInt32 ObjectTag, NewClient.UpdateTypes UpdateType, UInt16 Value)
            {
                this.ObjectTag = ObjectTag;
                this.UpdateType = UpdateType;
                this.DataType = UpdateDataTypes.UInt16;
                this.UpdateValueUInt16 = Value;
            }

            public UpdateItemEventArgs(UInt32 ObjectTag, NewClient.UpdateTypes UpdateType, UInt32 Value)
            {
                this.ObjectTag = ObjectTag;
                this.UpdateType = UpdateType;
                this.DataType = UpdateDataTypes.UInt32;
                this.UpdateValueUInt32 = Value;
            }

            public UpdateItemEventArgs(UInt32 ObjectTag, NewClient.UpdateTypes UpdateType, float Value)
            {
                this.ObjectTag = ObjectTag;
                this.UpdateType = UpdateType;
                this.DataType = UpdateDataTypes.Float;
                this.UpdateValueFloat = Value;
            }

            public UpdateItemEventArgs(UInt32 ObjectTag, NewClient.UpdateTypes UpdateType, string Value, string Plural)
            {
                this.ObjectTag = ObjectTag;
                this.UpdateType = UpdateType;
                this.DataType = UpdateDataTypes.String;
                this.UpdateString = Value;
                this.UpdateStringPlural = Plural;
            }

            public enum UpdateDataTypes
            {
                UInt8,
                UInt16,
                UInt32,
                Float,
                String,
            }

            public UInt32 ObjectTag { get; }
            public NewClient.UpdateTypes UpdateType { get; }
            public UpdateDataTypes DataType { get; }
            public float UpdateValueFloat { get; }
            public byte UpdateValueUInt8 { get; }
            public UInt16 UpdateValueUInt16 { get; }
            public UInt32 UpdateValueUInt32 { get; }
            public string UpdateString { get; }
            public string UpdateStringPlural { get; }

            public string ValueAsString
            {
                get
                {
                    switch (DataType)
                    {
                        case UpdateDataTypes.UInt8:
                            return this.UpdateValueUInt8.ToString();

                        case UpdateDataTypes.UInt16:
                            return this.UpdateValueUInt16.ToString();

                        case UpdateDataTypes.UInt32:
                            return this.UpdateValueUInt32.ToString();

                        case UpdateDataTypes.Float:
                            return this.UpdateValueFloat.ToString();

                        case UpdateDataTypes.String:
                            return this.UpdateString;

                        default:
                            return string.Empty;
                    }
                }
            }
        }

        public class BeginEndUpdateItemUpdateEventArgs : MessageHandlerEventArgs
        {
            public UInt32 ObjectTag { get; set; }
        }

        public class DeleteItemEventArgs : BatchEventArgs
        {
            public UInt32 ObjectTag { get; set; }
        }

        public class DeleteInventoryEventArgs : MessageHandlerEventArgs
        {
            public int ObjectTag { get; set; }
        }
    }
}

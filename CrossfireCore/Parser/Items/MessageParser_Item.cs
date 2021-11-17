using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.Parser
{
    public partial class MessageParserBase
    {
        protected abstract void HandleItem2(UInt32 item_location, UInt32 item_tag, UInt32 item_flags,
            UInt32 item_weight, UInt32 item_face, string item_name, string item_name_plural, UInt16 item_anim, byte item_animspeed,
            UInt32 item_nrof, UInt16 item_type);
        protected abstract void HandleBeginItem2();
        protected abstract void HandleEndItem2();

        protected abstract void HandleUpdateItem(UInt32 ObjectTag, NewClient.UpdateTypes UpdateType, Int64 UpdateValue);
        protected abstract void HandleUpdateItem(UInt32 ObjectTag, NewClient.UpdateTypes UpdateType, string UpdateValue, string UpdateValuePlural);
        protected abstract void HandleBeginUpdateItem(UInt32 ObjectTag);
        protected abstract void HandleEndUpdateItem(UInt32 ObjectTag);
        protected abstract void HandleDeleteItem(UInt32 ObjectTag);
        protected abstract void HandleBeginDeleteItem();
        protected abstract void HandleEndDeleteItem();
        /// <summary>
        /// Delete items carried in/by the object tag.
        /// Tag of 0 means to delete all items on the space the character is standing on.
        /// </summary>
        /// <param name="Tag"></param>
        protected abstract void HandleDeleteInventory(int ObjectTag);

        const float FLOAT_MULTF = 100000.0f;

        private void AddItemParsers()
        {
            AddCommandHandler("item2", new CommandParserDefinition(Parse_item2));
            AddCommandHandler("upditem", new CommandParserDefinition(Parse_upditem));
            AddCommandHandler("delitem", new CommandParserDefinition(Parse_delitem));
            AddCommandHandler("delinv", new CommandParserDefinition(Parse_delinv));
        }

        private bool Parse_item2(byte[] Message, ref int DataOffset, int DataEnd)
        {
            HandleBeginItem2();

            var item_location = BufferTokenizer.GetUInt32(Message, ref DataOffset);
            while (DataOffset < DataEnd)
            {
                var item_tag = BufferTokenizer.GetUInt32(Message, ref DataOffset);
                var item_flags = BufferTokenizer.GetUInt32(Message, ref DataOffset);
                var item_weight = BufferTokenizer.GetUInt32(Message, ref DataOffset);
                var item_face = BufferTokenizer.GetUInt32(Message, ref DataOffset);
                var item_namelen = BufferTokenizer.GetByte(Message, ref DataOffset);
                var item_name_bytes = BufferTokenizer.GetBytes(Message, ref DataOffset, item_namelen);

                string item_name, item_name_plural;
                int item_name_offset;
                for (item_name_offset = 0; item_name_offset < item_name_bytes.Length && item_name_bytes[item_name_offset] != 0x00; item_name_offset++) { }

                if (item_name_offset < item_name_bytes.Length)
                {
                    item_name = Encoding.ASCII.GetString(item_name_bytes, 0, item_name_offset);
                    item_name_plural = Encoding.ASCII.GetString(item_name_bytes, item_name_offset + 1, item_name_bytes.Length - 1 - item_name_offset);
                }
                else
                {
                    item_name = Encoding.ASCII.GetString(item_name_bytes, 0, item_name_bytes.Length);
                    item_name_plural = item_name;
                }

                var item_anim = BufferTokenizer.GetUInt16(Message, ref DataOffset);
                var item_animspeed = BufferTokenizer.GetByte(Message, ref DataOffset);
                var item_nrof = BufferTokenizer.GetUInt32(Message, ref DataOffset);
                var item_type = BufferTokenizer.GetUInt16(Message, ref DataOffset);

                HandleItem2(item_location, item_tag, item_flags, item_weight, item_face,
                    item_name, item_name_plural, item_anim, item_animspeed, item_nrof, item_type);
            }

            HandleEndItem2();

            return true;
        }

        private bool Parse_upditem(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var update_item_type = (NewClient.UpdateTypes)BufferTokenizer.GetByte(Message, ref DataOffset);
            var update_item_tag = BufferTokenizer.GetUInt32(Message, ref DataOffset);

            HandleBeginUpdateItem(update_item_tag);

            while (DataOffset < DataEnd)
            {
                if (update_item_type.HasFlag(NewClient.UpdateTypes.Location))
                {
                    var update_item_location = BufferTokenizer.GetUInt32(Message, ref DataOffset);
                    HandleUpdateItem(update_item_tag, NewClient.UpdateTypes.Location, update_item_location);
                }

                if (update_item_type.HasFlag(NewClient.UpdateTypes.Flags))
                {
                    var update_item_flags = BufferTokenizer.GetUInt32(Message, ref DataOffset);
                    HandleUpdateItem(update_item_tag, NewClient.UpdateTypes.Flags, update_item_flags);
                }

                if (update_item_type.HasFlag(NewClient.UpdateTypes.Weight))
                {
                    var update_item_weight = BufferTokenizer.GetUInt32(Message, ref DataOffset);
                    HandleUpdateItem(update_item_tag, NewClient.UpdateTypes.Weight, update_item_weight);
                }

                if (update_item_type.HasFlag(NewClient.UpdateTypes.Face))
                {
                    var update_item_face = BufferTokenizer.GetUInt32(Message, ref DataOffset);
                    HandleUpdateItem(update_item_tag, NewClient.UpdateTypes.Face, update_item_face);
                }

                if (update_item_type.HasFlag(NewClient.UpdateTypes.Name))
                {
                    var update_item_namelen = BufferTokenizer.GetByte(Message, ref DataOffset);
                    var update_item_name_bytes = BufferTokenizer.GetBytes(Message, ref DataOffset, update_item_namelen);

                    string update_item_name, update_item_name_plural;
                    int update_item_name_offset;
                    for (update_item_name_offset = 0; update_item_name_offset < update_item_name_bytes.Length && update_item_name_bytes[update_item_name_offset] != 0x00; update_item_name_offset++) { }

                    if (update_item_name_offset < update_item_name_bytes.Length)
                    {
                        update_item_name = Encoding.ASCII.GetString(update_item_name_bytes, 0, update_item_name_offset);
                        update_item_name_plural = Encoding.ASCII.GetString(update_item_name_bytes, update_item_name_offset + 1, update_item_name_bytes.Length - 1 - update_item_name_offset);
                    }
                    else
                    {
                        update_item_name = Encoding.ASCII.GetString(update_item_name_bytes, 0, update_item_name_bytes.Length);
                        update_item_name_plural = update_item_name;
                    }

                    HandleUpdateItem(update_item_tag, NewClient.UpdateTypes.Name, update_item_name, update_item_name_plural);
                }

                if (update_item_type.HasFlag(NewClient.UpdateTypes.Animation))
                {
                    var update_item_anim = BufferTokenizer.GetUInt16(Message, ref DataOffset);
                    HandleUpdateItem(update_item_tag, NewClient.UpdateTypes.Animation, update_item_anim);
                }

                if (update_item_type.HasFlag(NewClient.UpdateTypes.AnimationSpeed))
                {
                    var update_item_animspeed = BufferTokenizer.GetByte(Message, ref DataOffset);
                    HandleUpdateItem(update_item_tag, NewClient.UpdateTypes.AnimationSpeed, update_item_animspeed);
                }

                if (update_item_type.HasFlag(NewClient.UpdateTypes.NumberOf))
                {
                    var update_item_nrof = BufferTokenizer.GetUInt32(Message, ref DataOffset);
                    HandleUpdateItem(update_item_tag, NewClient.UpdateTypes.NumberOf, update_item_nrof);
                }
            }

            HandleEndUpdateItem(update_item_tag);

            return true;
        }

        private bool Parse_delitem(byte[] Message, ref int DataOffset, int DataEnd)
        {
            HandleBeginDeleteItem();

            while (DataOffset < DataEnd)
            {
                var del_item_tag = BufferTokenizer.GetUInt32(Message, ref DataOffset);
                HandleDeleteItem(del_item_tag);
            }

            HandleEndDeleteItem();
            
            return true;
        }

        private bool Parse_delinv(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var del_inv_tag = BufferTokenizer.GetStringAsInt(Message, ref DataOffset, DataEnd);

            HandleDeleteInventory(del_inv_tag);

            return true;
        }
    }
}

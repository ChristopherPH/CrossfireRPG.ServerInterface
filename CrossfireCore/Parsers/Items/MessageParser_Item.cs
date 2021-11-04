using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
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
        protected abstract void HandleBeginUpdateItem();
        protected abstract void HandleEndUpdateItem();
        protected abstract void HandleDeleteItem(UInt32 ObjectTag);
        protected abstract void HandleBeginDeleteItem();
        protected abstract void HandleEndDeleteItem();
        /// <summary>
        /// Delete items carried in/by the object tag.
        /// Tag of 0 means to delete all items on the space the character is standing on.
        /// </summary>
        /// <param name="Tag"></param>
        protected abstract void HandleDeleteInventory(int ObjectTag);

        private void AddItemParsers()
        {
            AddCommandHandler("item2", Parse_item2);
            AddCommandHandler("upditem", Parse_upditem);
            AddCommandHandler("delitem", Parse_delitem);
            AddCommandHandler("delinv", Parse_delinv);
        }

        private bool Parse_item2(byte[] packet, ref int offset)
        {
            HandleBeginItem2();

            var item_location = BufferTokenizer.GetUInt32(packet, ref offset);
            while (offset < packet.Length)
            {
                var item_tag = BufferTokenizer.GetUInt32(packet, ref offset);
                var item_flags = BufferTokenizer.GetUInt32(packet, ref offset);
                var item_weight = BufferTokenizer.GetUInt32(packet, ref offset);
                var item_face = BufferTokenizer.GetUInt32(packet, ref offset);
                var item_namelen = BufferTokenizer.GetByte(packet, ref offset);
                var item_name_bytes = BufferTokenizer.GetBytes(packet, ref offset, item_namelen);

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

                var item_anim = BufferTokenizer.GetUInt16(packet, ref offset);
                var item_animspeed = BufferTokenizer.GetByte(packet, ref offset);
                var item_nrof = BufferTokenizer.GetUInt32(packet, ref offset);
                var item_type = BufferTokenizer.GetUInt16(packet, ref offset);

                HandleItem2(item_location, item_tag, item_flags, item_weight, item_face,
                    item_name, item_name_plural, item_anim, item_animspeed, item_nrof, item_type);
            }

            HandleEndItem2();

            return true;
        }

        private bool Parse_upditem(byte[] packet, ref int offset)
        {
            var update_item_type = (NewClient.UpdateTypes)BufferTokenizer.GetByte(packet, ref offset);
            var update_item_tag = BufferTokenizer.GetUInt32(packet, ref offset);

            HandleBeginUpdateItem();

            while (offset < packet.Length)
            {
                if (update_item_type.HasFlag(NewClient.UpdateTypes.Location))
                {
                    var update_item_location = BufferTokenizer.GetUInt32(packet, ref offset);
                    HandleUpdateItem(update_item_tag, NewClient.UpdateTypes.Location, update_item_location);
                }

                if (update_item_type.HasFlag(NewClient.UpdateTypes.Flags))
                {
                    var update_item_flags = BufferTokenizer.GetUInt32(packet, ref offset);
                    HandleUpdateItem(update_item_tag, NewClient.UpdateTypes.Flags, update_item_flags);
                }

                if (update_item_type.HasFlag(NewClient.UpdateTypes.Weight))
                {
                    var update_item_weight = BufferTokenizer.GetUInt32(packet, ref offset);
                    HandleUpdateItem(update_item_tag, NewClient.UpdateTypes.Weight, update_item_weight);
                }

                if (update_item_type.HasFlag(NewClient.UpdateTypes.Face))
                {
                    var update_item_face = BufferTokenizer.GetUInt32(packet, ref offset);
                    HandleUpdateItem(update_item_tag, NewClient.UpdateTypes.Face, update_item_face);
                }

                if (update_item_type.HasFlag(NewClient.UpdateTypes.Name))
                {
                    var update_item_namelen = BufferTokenizer.GetByte(packet, ref offset);
                    var update_item_name_bytes = BufferTokenizer.GetBytes(packet, ref offset, update_item_namelen);

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
                    var update_item_anim = BufferTokenizer.GetUInt16(packet, ref offset);
                    HandleUpdateItem(update_item_tag, NewClient.UpdateTypes.Animation, update_item_anim);
                }

                if (update_item_type.HasFlag(NewClient.UpdateTypes.AnimationSpeed))
                {
                    var update_item_animspeed = BufferTokenizer.GetByte(packet, ref offset);
                    HandleUpdateItem(update_item_tag, NewClient.UpdateTypes.AnimationSpeed, update_item_animspeed);
                }

                if (update_item_type.HasFlag(NewClient.UpdateTypes.NumberOf))
                {
                    var update_item_nrof = BufferTokenizer.GetUInt32(packet, ref offset);
                    HandleUpdateItem(update_item_tag, NewClient.UpdateTypes.NumberOf, update_item_nrof);
                }
            }

            HandleEndUpdateItem();

            return true;
        }

        private bool Parse_delitem(byte[] packet, ref int offset)
        {
            HandleBeginDeleteItem();

            while (offset < packet.Length)
            {
                var del_item_tag = BufferTokenizer.GetUInt32(packet, ref offset);
                HandleDeleteItem(del_item_tag);
            }

            HandleEndDeleteItem();
            
            return true;
        }

        private bool Parse_delinv(byte[] packet, ref int offset)
        {
            var del_inv_tag = BufferTokenizer.GetStringAsInt(packet, ref offset);

            HandleDeleteInventory(del_inv_tag);

            return true;
        }
    }
}

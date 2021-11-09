using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParserBase
    {
        protected abstract void HandleNewMap();

        /// <summary>
        /// Clear everything at the given x/y co-ords
        /// </summary>
        protected abstract void HandleMap2Clear(int x, int y);
#if THIS_IS_IN_THE_GTK_CLIENT
        protected abstract void HandleMap2ClearOld(int x, int y);
#endif
        protected abstract void HandleMap2Scroll(int x, int y);
        protected abstract void HandleMap2Darkness(int x, int y, byte darkness);

        /// <summary>
        /// Clear face, animation and smooth at the given x/y co-ords (face/animation was 0)
        /// </summary>
        protected abstract void HandleMap2ClearLayer(int x, int y, int layer);

        /// <summary>
        /// Add face and smoothing info at the given x/y co-ords
        /// </summary>
        protected abstract void HandleMap2Face(int x, int y, int layer, UInt16 face, byte smooth);

        /// <summary>
        /// Add animation and smoothing info at the given x/y co-ords
        /// </summary>
        protected abstract void HandleMap2Animation(int x, int y, int layer, UInt16 animation, 
            int animationtype, byte animationspeed, byte smooth);

        protected abstract void HandleSmooth(UInt16 face, UInt16 smooth);

        const int MAP2_COORD_OFFSET = 15;

        private void AddMapParsers()
        {
            AddCommandHandler("newmap", new ParseCommand(Parse_newmap));
            AddCommandHandler("map2", new ParseCommand(Parse_map2));
            AddCommandHandler("smooth", new ParseCommand(Parse_smooth));
        }

        private bool Parse_newmap(byte[] packet, ref int offset)
        {
            HandleNewMap();
            return true;
        }

        private bool Parse_map2(byte[] packet, ref int offset)
        {
            while (offset < packet.Length)
            {
                var map_coord = BufferTokenizer.GetUInt16(packet, ref offset);

                var map_coord_x = (map_coord >> 10) & 0x3F;         //top 6 bits
                var map_coord_y = (map_coord >> 4) & 0x3F;          //next 6 bits
                var map_coord_l = (map_coord) & 0x0F;               //bottom 4 bits //TODO: find a use for this variable

                map_coord_x -= MAP2_COORD_OFFSET;
                map_coord_y -= MAP2_COORD_OFFSET;

                //handle scroll
                if ((map_coord & 0x1) != 0)
                {
                    HandleMap2Scroll(map_coord_x, map_coord_y);
                    continue;
                }

#if THIS_IS_IN_THE_GTK_CLIENT
                        //clear/init space
                        HandleMap2ClearOld(map_coord_x, map_coord_y);
#endif

                while (offset < packet.Length)
                {
                    var map_len_type = BufferTokenizer.GetByte(packet, ref offset);

                    //0xff means next co-ord
                    if (map_len_type == 0xFF)
                        break;

                    var map_data_len = (map_len_type >> 5) & 0x07;      //top 3 bits
                    var map_data_type = (map_len_type) & 0x1F;          //bottom 5 bits

                    //currently unused
                    if (map_data_len == 0x07)
                    {
                        map_data_len += BufferTokenizer.GetByte(packet, ref offset);
                        throw new Exception("Invalid map_data_len: Multibyte len is unused: " + map_data_len.ToString());
                    }

                    switch (map_data_type)
                    {
                        case 0x00: //clear
                            if (map_data_len != 0)
                                throw new Exception("Invalid map_data_len: must be 0");

                            HandleMap2Clear(map_coord_x, map_coord_y);
                            break;

                        case 0x01: //darkness
                            if (map_data_len != 1)
                                throw new Exception("Invalid map_data_len: must be 1");

                            var darkness = BufferTokenizer.GetByte(packet, ref offset); //0=dark, 255=light

                            HandleMap2Darkness(map_coord_x, map_coord_y, darkness);
                            break;

                        case 0x10:  //lowest layer
                        case 0x11:
                        case 0x12:
                        case 0x13:
                        case 0x14:
                        case 0x15:
                        case 0x16:
                        case 0x17:
                        case 0x18:
                        case 0x19:    //highest layer
                            var layer = map_data_type - 0x10;

                            var face_or_animation = BufferTokenizer.GetUInt16(packet, ref offset);
                            var is_animation = (face_or_animation >> 15) != 0; //high bit set

                            byte animspeed = 0;
                            byte smooth = 0;

                            switch (map_data_len)
                            {
                                case 2: //no smooth information
                                    break;

                                case 3:
                                    if (is_animation)
                                        animspeed = BufferTokenizer.GetByte(packet, ref offset);
                                    else
                                        smooth = BufferTokenizer.GetByte(packet, ref offset);
                                    break;

                                case 4:
                                    animspeed = BufferTokenizer.GetByte(packet, ref offset);
                                    smooth = BufferTokenizer.GetByte(packet, ref offset);
                                    break;

                                default:
                                    throw new Exception("Invalid map_data_len - must be 2,3,4: is " + map_data_len.ToString());
                            }

                            if (face_or_animation == 0)
                            {
                                HandleMap2ClearLayer(map_coord_x, map_coord_y, layer);
                            }
                            else if (is_animation)
                            {
                                var anim_type = (face_or_animation >> 6) & 0x03;      //top 2 bits

                                var animation = (UInt16)(face_or_animation & 0x1FFF);
                                HandleMap2Animation(map_coord_x, map_coord_y, layer, animation, anim_type, animspeed, smooth);
                            }
                            else
                            {
                                HandleMap2Face(map_coord_x, map_coord_y, layer, face_or_animation, smooth);
                            }
                            break;

                        default:
                            throw new Exception("Invalid map_data_type: " + map_data_type.ToString());
                    }
                }
            }

            return true;
        }

        private bool Parse_smooth(byte[] packet, ref int offset)
        {
            var face = BufferTokenizer.GetUInt16(packet, ref offset);
            var smoothpic = BufferTokenizer.GetUInt16(packet, ref offset);
            HandleSmooth(face, smoothpic);
            
            return true;
        }
    }
}

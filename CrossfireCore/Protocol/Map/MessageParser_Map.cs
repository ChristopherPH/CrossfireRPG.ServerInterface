/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using CrossfireRPG.ServerInterface.Definitions;
using System;

namespace CrossfireRPG.ServerInterface.Protocol
{
    public partial class MessageParser
    {
        protected abstract void HandleNewMap();

        protected abstract void HandleMap2Begin();
        protected abstract void HandleMap2End();

        protected abstract void HandleMap2BeginLocation(int x, int y);
        protected abstract void HandleMap2EndLocation(int x, int y);

        /// <summary>
        /// Clear everything at the given x/y co-ords
        /// </summary>
        protected abstract void HandleMap2Clear(int x, int y);
        protected abstract void HandleMap2Scroll(int x, int y);
        protected abstract void HandleMap2Darkness(int x, int y, byte darkness);
        protected abstract void HandleMap2Label(int x, int y, NewClient.Map2Type_Label labelType, string label);

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
            int animationflags, byte animationspeed, byte smooth);

        protected abstract void HandleSmooth(UInt16 face, UInt16 smooth);

        const int MAP2_COORD_OFFSET = 15;

        private void AddMapParsers()
        {
            AddCommandHandler("newmap", new CommandParserDefinition(Parse_newmap));
            AddCommandHandler("map2", new CommandParserDefinition(Parse_map2));
            AddCommandHandler("smooth", new CommandParserDefinition(Parse_smooth));
        }

        private bool Parse_newmap(byte[] Message, ref int DataOffset, int DataEnd)
        {
            HandleNewMap();
            return true;
        }

        private bool Parse_map2(byte[] Message, ref int DataOffset, int DataEnd)
        {
            HandleMap2Begin();

            while (DataOffset < DataEnd)
            {
                var map_coord = BufferTokenizer.GetUInt16(Message, ref DataOffset);

                var map_coord_x = (map_coord >> 10) & 0x3F;         //top 6 bits
                var map_coord_y = (map_coord >> 4) & 0x3F;          //next 6 bits
                var map_coord_l = (map_coord) & 0x0F;               //bottom 4 bits: 0=normal co-ord, 1 = scroll, 2=15 = unused

                map_coord_x -= MAP2_COORD_OFFSET;
                map_coord_y -= MAP2_COORD_OFFSET;

                //handle scroll
                if ((map_coord_l & 0x1) != 0)
                {
                    HandleMap2Scroll(map_coord_x, map_coord_y);
                    continue;
                }

                HandleMap2BeginLocation(map_coord_x, map_coord_y);

                while (DataOffset < DataEnd)
                {
                    var map_len_type = BufferTokenizer.GetByte(Message, ref DataOffset);

                    //0xff means next co-ord
                    if (map_len_type == 0xFF)
                        break;

                    var map_data_len = (map_len_type >> 5) & 0x07;                  //top 3 bits are the data length
                    var map2_type = (NewClient.Map2Type)((map_len_type) & 0x1F);    //bottom 5 bits is the map2_type

                    //A length of 7 indicates the next byte holds the full length
                    //for server protocol 1030+. Previous protocol versions did
                    //not implement this, but the comments were ambiguous on how
                    //to handle this case (was the length additive or explicitly
                    //set). So, throw an exception if the protocol version is
                    //incorrect and the parser encounters a length of 7.
                    if (map_data_len == 0x07)
                    {
                        if (ServerProtocolVersion < 1030)
                            throw new MessageParserException("Invalid map_data_len: Multibyte len is unused: " + map_data_len.ToString());

                        map_data_len = BufferTokenizer.GetByte(Message, ref DataOffset);
                    }

                    switch (map2_type)
                    {
                        case NewClient.Map2Type.Clear:
                            if (map_data_len != 0)
                                throw new MessageParserException("Invalid map_data_len: must be 0 for clear");

                            HandleMap2Clear(map_coord_x, map_coord_y);
                            break;

                        case NewClient.Map2Type.Darkness:
                            if (map_data_len != 1)
                                throw new MessageParserException("Invalid map_data_len: must be 1 for darkness");

                            var darkness = BufferTokenizer.GetByte(Message, ref DataOffset); //0=dark, 255=light

                            HandleMap2Darkness(map_coord_x, map_coord_y, darkness);
                            break;

                        case NewClient.Map2Type.Label:
                            if (map_data_len <= 1)
                                throw new MessageParserException("Invalid map_data_len: must > 1 for label");

                            var labelType = BufferTokenizer.GetByte(Message, ref DataOffset);
                            var labelLen = BufferTokenizer.GetByte(Message, ref DataOffset);
                            var label = BufferTokenizer.GetBytesAsString(Message, ref DataOffset, labelLen);

                            HandleMap2Label(map_coord_x, map_coord_y, (NewClient.Map2Type_Label)labelType, label);
                            break;

                        case NewClient.Map2Type.Layer_1:
                        case NewClient.Map2Type.Layer_2:
                        case NewClient.Map2Type.Layer_3:
                        case NewClient.Map2Type.Layer_4:
                        case NewClient.Map2Type.Layer_5:
                        case NewClient.Map2Type.Layer_6:
                        case NewClient.Map2Type.Layer_7:
                        case NewClient.Map2Type.Layer_8:
                        case NewClient.Map2Type.Layer_9:
                        case NewClient.Map2Type.Layer_10:
                            var layer = map2_type - NewClient.Map2Type.Layer_1;

                            var face_or_animation = BufferTokenizer.GetUInt16(Message, ref DataOffset);
                            var is_animation = (face_or_animation >> 15) != 0; //high bit set

                            byte animspeed = 0;
                            byte smooth = 0;

                            switch (map_data_len)
                            {
                                case 2: //no smooth information
                                    break;

                                case 3:
                                    if (is_animation)
                                        animspeed = BufferTokenizer.GetByte(Message, ref DataOffset);
                                    else
                                        smooth = BufferTokenizer.GetByte(Message, ref DataOffset);
                                    break;

                                case 4:
                                    animspeed = BufferTokenizer.GetByte(Message, ref DataOffset);
                                    smooth = BufferTokenizer.GetByte(Message, ref DataOffset);
                                    break;

                                default:
                                    throw new MessageParserException("Invalid map_data_len - must be 2,3,4: is " + map_data_len.ToString());
                            }

                            if (face_or_animation == 0)
                            {
                                HandleMap2ClearLayer(map_coord_x, map_coord_y, layer);
                            }
                            else if (is_animation)
                            {
                                var anim_flags = (face_or_animation >> 6) & 0x03;      //top 2 bits

                                var animation = (UInt16)(face_or_animation & 0x1FFF);
                                HandleMap2Animation(map_coord_x, map_coord_y, layer, animation, anim_flags, animspeed, smooth);
                            }
                            else
                            {
                                HandleMap2Face(map_coord_x, map_coord_y, layer, face_or_animation, smooth);
                            }
                            break;

                        default:
                            throw new MessageParserException("Invalid map_data_type: " + map2_type.ToString());
                    }
                }

                HandleMap2EndLocation(map_coord_x, map_coord_y);
            }

            HandleMap2End();

            return true;
        }

        private bool Parse_smooth(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var face = BufferTokenizer.GetUInt16(Message, ref DataOffset);
            var smoothpic = BufferTokenizer.GetUInt16(Message, ref DataOffset);
            HandleSmooth(face, smoothpic);

            return true;
        }
    }
}

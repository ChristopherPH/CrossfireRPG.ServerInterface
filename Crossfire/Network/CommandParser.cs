﻿using Crossfire.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire
{
    public abstract class CommandParser
    {
        const int MAP2_COORD_OFFSET = 15;
        const float FLOAT_MULTF = 100000.0f;

        protected abstract void HandleVersion(int csval, int scval, string verstring);
        protected abstract void HandleFailure(string ProtocolCommand, string FailureString);

        protected abstract void HandleAddmeSuccess();
        protected abstract void HandleAddmeFailed();
        protected abstract void HandleGoodbye();
        protected abstract void HandleNewMap();

        /// <summary>
        /// Delete items carried in/by the object tag.
        /// Tag of 0 means to delete all items on the space the character is standing on.
        /// </summary>
        /// <param name="Tag"></param>
        protected abstract void HandleDeleteInventory(int ObjectTag);
        protected abstract void HandleQuery(int Flags, string QueryText);

        protected abstract void HandleStat(NewClient.CharacterStats Stat, Int64 Value);
        protected abstract void HandleStat(NewClient.CharacterStats Stat, string Value);
        protected abstract void HandleStat(NewClient.CharacterStats Stat, float Value);

        protected abstract void HandleSkill(int Skill, Int64 Value);

        protected abstract void HandleSmooth(UInt16 face, UInt16 smooth);
        protected abstract void HandleAnimation(UInt16 AnimationNumber, UInt16 AnimationFlags, 
            UInt16[] AnimationFaces);

        protected abstract void HandleItem2(UInt32 item_location, UInt32 item_tag, UInt32 item_flags, 
            UInt32 item_weight, UInt32 item_face, string item_name, UInt16 item_anim, byte item_animspeed, 
            UInt32 item_nrof, UInt16 item_type);

        protected abstract void HandleImage2(UInt32 image_face, byte image_faceset, byte[] image_png);

        protected abstract void HandleDrawExtInfo(NewClient.NewDrawInfo Colour, NewClient.MsgTypes MessageType, int SubType, string Message);

        protected abstract void HandleMap2Clear(int x, int y);
        protected abstract void HandleMap2Darkness(int x, int y, byte darkness);
        protected abstract void HandleMap2ClearAnimationSmooth(int x, int y, int layer);
        protected abstract void HandleMap2Face(int x, int y, int layer, UInt16 face, byte smooth);
        protected abstract void HandleMap2Animation(int x, int y, int layer, UInt16 animation, int animationtype, byte animationspeed, byte smooth);

        public void ParsePacket(object sender, ConnectionPacketEventArgs e)
        {
            System.Diagnostics.Debug.Assert(e != null);
            System.Diagnostics.Debug.Assert(e.Packet != null);
            System.Diagnostics.Debug.Assert(e.Packet.Length > 0);

            int offset = 0;
            var cmd = Tokenizer.GetString(e.Packet, ref offset);

            System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(cmd));

            Logger.Log(Logger.Levels.Debug, "S->C: cmd={0}, datalen={1}", cmd, e.Packet.Length - offset);
            Logger.Log(Logger.Levels.Comm, "{0}", HexDump.Utils.HexDump(e.Packet));


            switch (cmd)
            {
                case "version":
                    var version_csval = Tokenizer.GetStringAsInt(e.Packet, ref offset);
                    var version_scval = Tokenizer.GetStringAsInt(e.Packet, ref offset);
                    var version_verstr = Tokenizer.GetRemainingBytesAsString(e.Packet, ref offset);
                    
                    HandleVersion(version_csval, version_scval, version_verstr);
                    break;

                case "failure":
                    var protocol_command = Tokenizer.GetString(e.Packet, ref offset);
                    var failure_string = Tokenizer.GetRemainingBytesAsString(e.Packet, ref offset);

                    HandleFailure(protocol_command, failure_string);
                    Logger.Log(Logger.Levels.Error, "Failure: {0} {1}", protocol_command, failure_string);
                    break;

                case "addme_success":
                    HandleAddmeSuccess();
                    break;

                case "addme_failed":
                    HandleAddmeFailed();
                    break;

                case "goodbye":
                    HandleGoodbye();
                    break;

                case "newmap":
                    HandleNewMap();
                    break;

                case "delinv":
                    var tag = Tokenizer.GetStringAsInt(e.Packet, ref offset);
                    HandleDeleteInventory(tag);
                    break;

                case "accountplayers":
                    var num_characters = Tokenizer.GetByte(e.Packet, ref offset);

                    while (offset < e.Packet.Length)
                    {
                        var char_data_len = Tokenizer.GetByte(e.Packet, ref offset);
                        if (char_data_len == 0)
                            break;

                        var char_data_type = (NewClient.AccountCharacterLoginTypes)Tokenizer.GetByte(e.Packet, ref offset);
                        char_data_len--;

                        switch (char_data_type)
                        {
                            case NewClient.AccountCharacterLoginTypes.Level:
                            case NewClient.AccountCharacterLoginTypes.FaceNum:
                                var char_data_int = Tokenizer.GetUInt16(e.Packet, ref offset);
                                break;

                            case NewClient.AccountCharacterLoginTypes.Name:
                            case NewClient.AccountCharacterLoginTypes.Class:
                            case NewClient.AccountCharacterLoginTypes.Race:
                            case NewClient.AccountCharacterLoginTypes.Face:
                            case NewClient.AccountCharacterLoginTypes.Party:
                            case NewClient.AccountCharacterLoginTypes.Map:
                                var char_data_str = Tokenizer.GetBytesAsString(e.Packet, ref offset, char_data_len);
                                break;

                            default:
                                Tokenizer.GetBytes(e.Packet, ref offset, char_data_len);
                                break;
                        }
                    }
                    break;

                case "query":
                    var query_flags = Tokenizer.GetStringAsInt(e.Packet, ref offset);
                    var query_text = Tokenizer.GetRemainingBytesAsString(e.Packet, ref offset);
                    HandleQuery(query_flags, query_text);
                    break;

                case "stats":
                    while (offset < e.Packet.Length)
                    {
                        var stat_number = Tokenizer.GetByte(e.Packet, ref offset);

                        switch ((NewClient.CharacterStats)stat_number)
                        {
                            case NewClient.CharacterStats.Range:
                            case NewClient.CharacterStats.Title:
                                var stat_len = Tokenizer.GetByte(e.Packet, ref offset);
                                var stat_text = Tokenizer.GetBytesAsString(e.Packet, ref offset, stat_len);
                                HandleStat((NewClient.CharacterStats)stat_number, stat_text);
                                break;

                            case NewClient.CharacterStats.Speed:
                            case NewClient.CharacterStats.WeapSp:
                                var stat_32f = Tokenizer.GetUInt32(e.Packet, ref offset);

                                HandleStat((NewClient.CharacterStats)stat_number, stat_32f / FLOAT_MULTF);
                                break;

                            case NewClient.CharacterStats.WeightLim:
                                var stat_32 = Tokenizer.GetUInt32(e.Packet, ref offset);
                                HandleStat((NewClient.CharacterStats)stat_number, stat_32);
                                break;

                            case NewClient.CharacterStats.Exp64:
                                var stat_64 = Tokenizer.GetUInt64(e.Packet, ref offset);
                                HandleStat((NewClient.CharacterStats)stat_number, stat_64);
                                break;

                            default:
                                if ((stat_number >= NewClient.CharacterStats_SkillInfo) &&
                                    (stat_number < NewClient.CharacterStats_SkillInfo + NewClient.CharacterStats_NumSkills))
                                {
                                    var skill_32 = Tokenizer.GetUInt32(e.Packet, ref offset);
                                    HandleSkill(stat_number - NewClient.CharacterStats_SkillInfo, skill_32);
                                }
                                else
                                {
                                    var stat_value = Tokenizer.GetUInt16(e.Packet, ref offset);
                                    HandleStat((NewClient.CharacterStats)stat_number, stat_value);
                                }
                                break;
                        }

                    }
                    break;

                case "smooth":
                    var face = Tokenizer.GetUInt16(e.Packet, ref offset);
                    var smoothpic = Tokenizer.GetUInt16(e.Packet, ref offset);
                    HandleSmooth(face, smoothpic);
                    break;

                case "anim":
                    var anim_num = Tokenizer.GetUInt16(e.Packet, ref offset);
                    var anim_flags = Tokenizer.GetUInt16(e.Packet, ref offset);
                    var anim_faces = new UInt16[e.Packet.Length - offset];

                    int anim_offset = 0;
                    while (offset < e.Packet.Length)
                        anim_faces[anim_offset++] = Tokenizer.GetUInt16(e.Packet, ref offset);

                    HandleAnimation(anim_num, anim_flags, anim_faces);
                    break;

                case "item2":
                    var item_location = Tokenizer.GetUInt32(e.Packet, ref offset);
                    while (offset < e.Packet.Length)
                    {
                        var item_tag = Tokenizer.GetUInt32(e.Packet, ref offset);
                        var item_flags = Tokenizer.GetUInt32(e.Packet, ref offset);
                        var item_weight = Tokenizer.GetUInt32(e.Packet, ref offset);
                        var item_face = Tokenizer.GetUInt32(e.Packet, ref offset);
                        var item_namelen = Tokenizer.GetByte(e.Packet, ref offset);
                        var item_name = Tokenizer.GetBytesAsString(e.Packet, ref offset, item_namelen);
                        var item_anim = Tokenizer.GetUInt16(e.Packet, ref offset);
                        var item_animspeed = Tokenizer.GetByte(e.Packet, ref offset);
                        var item_nrof = Tokenizer.GetUInt32(e.Packet, ref offset);
                        var item_type = Tokenizer.GetUInt16(e.Packet, ref offset);

                        HandleItem2(item_location, item_tag, item_flags, item_weight, item_face, 
                            item_name, item_anim, item_animspeed, item_nrof, item_type);
                    }
                    break;

                case "image2":
                    var image_face = Tokenizer.GetUInt32(e.Packet, ref offset);
                    var image_faceset = Tokenizer.GetByte(e.Packet, ref offset);
                    var image_len = Tokenizer.GetUInt32(e.Packet, ref offset);
                    var image_png = Tokenizer.GetBytes(e.Packet, ref offset, (int)image_len);

                    HandleImage2(image_face, image_faceset, image_png);
                    break;

                case "map2":
                    while (offset < e.Packet.Length)
                    {
                        var map_coord = Tokenizer.GetUInt16(e.Packet, ref offset);

                        var map_coord_x = (map_coord >> 10) & 0x3F;         //top 6 bits
                        var map_coord_y = (map_coord >> 4) & 0x3F;          //next 6 bits
                        var map_coord_l = (map_coord) & 0x0F;               //bottom 4 bits

                        map_coord_x -= MAP2_COORD_OFFSET;
                        map_coord_y -= MAP2_COORD_OFFSET;

                        while (offset < e.Packet.Length)
                        {
                            var map_len_type = Tokenizer.GetByte(e.Packet, ref offset);

                            //0xff means next co-ord
                            if (map_len_type == 0xFF)
                                break;

                            var map_data_len = (map_len_type >> 5) & 0x07;      //top 3 bits
                            var map_data_type = (map_len_type) & 0x1F;          //bottom 5 bits

                            //currently unused
                            if (map_data_len == 0x07)
                            {
                                map_data_len += Tokenizer.GetByte(e.Packet, ref offset);
                                throw new Exception("apparently unused");
                            }

                            switch (map_data_type)
                            {
                                case 0x00: //clear
                                    if (map_data_len != 0)
                                        throw new Exception();
                                    HandleMap2Clear(map_coord_x, map_coord_y);
                                    break;

                                case 0x01: //darkness
                                    if (map_data_len != 1)
                                        throw new Exception();

                                    var darkness = Tokenizer.GetByte(e.Packet, ref offset); //0=dark, 255=light

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

                                    var face_or_animation = Tokenizer.GetUInt16(e.Packet, ref offset);
                                    var is_animation = (face_or_animation & 0x8000) != 0; //high bit set

                                    byte animspeed = 0;
                                    byte smooth = 0;

                                    switch (map_data_len)
                                    {
                                        case 2:
                                            break;

                                        case 3:
                                            if (is_animation)
                                                animspeed = Tokenizer.GetByte(e.Packet, ref offset);
                                            else
                                                smooth = Tokenizer.GetByte(e.Packet, ref offset);
                                            break;

                                        case 4:
                                            animspeed = Tokenizer.GetByte(e.Packet, ref offset);
                                            smooth = Tokenizer.GetByte(e.Packet, ref offset);
                                            break;

                                        default:
                                            throw new Exception();
                                    }

                                    if (face_or_animation == 0)
                                    {
                                        HandleMap2ClearAnimationSmooth(map_coord_x, map_coord_y, layer);
                                    }
                                    else if (is_animation)
                                    {
                                        var anim_type = (face_or_animation >> 6) & 0x03;      //top 2 bits

                                        HandleMap2Animation(map_coord_x, map_coord_y, layer, face_or_animation, anim_type, animspeed, smooth);
                                    }
                                    else
                                    {
                                        HandleMap2Face(map_coord_x, map_coord_y, layer, face_or_animation, smooth);
                                    }
                                    break;

                                default:
                                    throw new Exception();
                            }
                        }
                    }
                    break;


                case "drawextinfo":
                    var colour = (NewClient.NewDrawInfo)Tokenizer.GetStringAsInt(e.Packet, ref offset);
                    var message_type = (NewClient.MsgTypes)Tokenizer.GetStringAsInt(e.Packet, ref offset);
                    var sub_type = Tokenizer.GetStringAsInt(e.Packet, ref offset);
                    var message = Tokenizer.GetRemainingBytesAsString(e.Packet, ref offset);

                    HandleDrawExtInfo(colour, message_type, sub_type, message);

                    /*
                    NewClient.MsgTypeAdmin admin_type = NewClient.MsgTypeAdmin.Error;

                    switch (message_type)
                    {
                        case NewClient.MsgTypes.Admin:
                            admin_type = (NewClient.MsgTypeAdmin)sub_type;
                            break;

                        case NewClient.MsgTypes.MOTD: //no subtype
                            break;
                    }
                    Logger.Log(Logger.Levels.Info, "{0}\n{1}", admin_type, message);
                    */
                    break;

                default:
                    Logger.Log(Logger.Levels.Warn, "Unhandled Command: {0}", cmd);
                    break;
            }

            //log excess data
            if (offset < e.Packet.Length)
            {
                Logger.Log(Logger.Levels.Warn, "Excess Data:\n{0}",
                    HexDump.Utils.HexDump(e.Packet, offset));
            }
        }
    }
}

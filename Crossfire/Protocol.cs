using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire
{
    public static class Protocol
    {
        public static Connection Connection
        {
            get => _Connection;
            set
            {
                if (_Connection == value)
                    return;

                if (_Connection != null)
                {
                    _Connection.OnPacket -= ParsePacket;
                    _Connection.OnStatusChanged -= ConnectionStatusChanged;
                }

                _Connection = value;

                if (_Connection != null)
                {
                    _Connection.OnPacket += ParsePacket;
                    _Connection.OnStatusChanged += ConnectionStatusChanged;

                    //if (_Connection.ConnectionStatus == ConnectionStatuses.Connected)
                    //    AddClient();
                }
            }
        }
        private static Connection _Connection;



        private static void ConnectionStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            System.Diagnostics.Debug.Assert(e != null);

            switch (e.Status)
            {
                case ConnectionStatuses.Disconnected:
                    break;

                case ConnectionStatuses.Connecting:
                    break;

                case ConnectionStatuses.Connected:
                    //AddClient();
                    break;
            }
        }

        private static void AddClient()
        {
            _Connection.SendMessage("version 1023 1029 Chris Client");
            _Connection.SendMessage("addme");
            //_Connection.SendMessage("bad command");
        }

        private static void Login()
        {

        }

        const int MAP2_COORD_OFFSET = 15;

        private static void ParsePacket(object sender, ConnectionPacketEventArgs e)
        {
            System.Diagnostics.Debug.Assert(e != null);
            System.Diagnostics.Debug.Assert(e.Packet != null);
            System.Diagnostics.Debug.Assert(e.Packet.Length > 0);

            int offset = 0;
            var cmd = Tokenizer.GetString(e.Packet, ref offset);

            Logger.Log(Logger.Levels.Debug, "S->C: cmd={0}, datalen={1}", cmd, e.Packet.Length - offset);
            Logger.Log(Logger.Levels.Comm, "{0}", HexDump.Utils.HexDump(e.Packet));


            switch (cmd)
            {
                case "version":
                    var csval = Tokenizer.GetStringAsInt(e.Packet, ref offset);
                    var scval = Tokenizer.GetStringAsInt(e.Packet, ref offset);
                    var verstr = Tokenizer.GetRemainingBytesAsString(e.Packet, ref offset);

                    //verify version
                    AddClient();
                    break;

                case "failure":
                    var protocol_command = Tokenizer.GetString(e.Packet, ref offset);
                    var failure_string = Tokenizer.GetRemainingBytesAsString(e.Packet, ref offset);
                    Logger.Log(Logger.Levels.Error, "Failure: {0} {1}", protocol_command, failure_string);
                    break;

                case "addme_success":
                    break;

                case "addme_failed":
                    break;

                case "goodbye":
                    break;

                case "newmap":
                    break;

                case "delinv":
                    var tag = Tokenizer.GetStringAsInt(e.Packet, ref offset);
                    break;

                case "query":
                    var query_flags = Tokenizer.GetStringAsInt(e.Packet, ref offset);
                    var query_text = Tokenizer.GetRemainingBytesAsString(e.Packet, ref offset);
                    break;

                case "stats":
                    while (offset < e.Packet.Length)
                    {
                        var stat_number = Tokenizer.GetByte(e.Packet, ref offset);

                        switch (stat_number)
                        {
                            case 20: //range
                            case 21: //title
                                var stat_namelen = Tokenizer.GetByte(e.Packet, ref offset);
                                var stat_name = Tokenizer.GetBytesAsString(e.Packet, ref offset, stat_namelen);
                                break;

                            case 17: //speed
                            case 19: //weap speed
                            case 26: //weight limit
                                var stat_32 = Tokenizer.GetUInt32(e.Packet, ref offset);
                                break;

                            case 28: //exp
                                var stat_64_1 = Tokenizer.GetUInt32(e.Packet, ref offset);
                                var stat_64_2 = Tokenizer.GetUInt32(e.Packet, ref offset);
                                break;

                            default:
                                var stat_value = Tokenizer.GetUInt16(e.Packet, ref offset);
                            break;
                        }
                        
                    }
                    break;

                case "smooth":
                    var face = Tokenizer.GetUInt16(e.Packet, ref offset);
                    var smoothpic = Tokenizer.GetUInt16(e.Packet, ref offset);
                    break;

                case "anim":
                    var anim_num = Tokenizer.GetUInt16(e.Packet, ref offset);
                    var anim_flags = Tokenizer.GetUInt16(e.Packet, ref offset);
                    while (offset < e.Packet.Length)
                    {
                        var anim_face = Tokenizer.GetUInt16(e.Packet, ref offset);
                    }
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
                    }
                    break;

                case "image2":
                    var image_face = Tokenizer.GetUInt32(e.Packet, ref offset);
                    var image_faceset = Tokenizer.GetByte(e.Packet, ref offset);
                    var image_len = Tokenizer.GetUInt32(e.Packet, ref offset);
                    var image_png = Tokenizer.GetBytes(e.Packet, ref offset, (int)image_len);
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
                                    break;

                                case 0x01: //darkness
                                    if (map_data_len != 1)
                                        throw new Exception();

                                    var darkness = Tokenizer.GetByte(e.Packet, ref offset); //0=dark, 255=light
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

                                    var animspeed = 0;
                                    var smooth = 0;

                                    if (map_data_len == 2)
                                    {

                                    }
                                    else if (map_data_len == 3)
                                    {
                                        if ((face_or_animation & 0x8000) != 0) //high bit set
                                        {
                                            animspeed = Tokenizer.GetByte(e.Packet, ref offset);
                                        }
                                        else
                                        {
                                            smooth = Tokenizer.GetByte(e.Packet, ref offset);
                                        }
                                    }
                                    else if (map_data_len == 4)
                                    {
                                        animspeed = Tokenizer.GetByte(e.Packet, ref offset);
                                        smooth = Tokenizer.GetByte(e.Packet, ref offset);
                                    }
                                    else
                                    {
                                        throw new Exception();
                                    }

                                    if (face_or_animation == 0)
                                    {
                                        //clear anim
                                    }

                                    if ((face_or_animation & 0x8000) != 0) //high bit set
                                    {
                                        var anim_type = (face_or_animation >> 6) & 0x03;      //top 2 bits
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

                    NewClient.MsgTypeAdmin admin_type = NewClient.MsgTypeAdmin.Error;

                    switch (message_type)
                    {
                        case NewClient.MsgTypes.Admin:
                            admin_type = (NewClient.MsgTypeAdmin)sub_type;
                            break;
                    }
                    
                    var message = Tokenizer.GetRemainingBytesAsString(e.Packet, ref offset);

                    Logger.Log(Logger.Levels.Info, "{0}\n{1}", admin_type, message);

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

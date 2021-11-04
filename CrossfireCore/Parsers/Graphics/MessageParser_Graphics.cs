using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParserBase
    {
        protected abstract void HandleAnimation(UInt16 AnimationNumber, UInt16 AnimationFlags,
            UInt16[] AnimationFaces);
        protected abstract void HandleDrawExtInfo(NewClient.NewDrawInfo Flags,
            NewClient.MsgTypes MessageType, int SubType, string Message);
        protected abstract void HandleImage2(UInt32 image_face, byte image_faceset, byte[] image_png);
        protected abstract void HandleMagicMap(int Width, int Height, int PlayerX, int PlayerY, byte[] MapData);

        private void AddGraphicsParsers()
        {
            AddCommandHandler("anim", Parse_anim);
            AddCommandHandler("drawextinfo", Parse_drawextinfo);
            AddCommandHandler("image2", Parse_image2);
            AddCommandHandler("magicmap", Parse_magicmap);
        }

        private bool Parse_anim(byte[] packet, ref int offset)
        {
            var anim_num = BufferTokenizer.GetUInt16(packet, ref offset);
            var anim_flags = BufferTokenizer.GetUInt16(packet, ref offset);
            var anim_faces = new UInt16[(packet.Length - offset) / 2];

            int anim_offset = 0;
            while (offset < packet.Length)
                anim_faces[anim_offset++] = BufferTokenizer.GetUInt16(packet, ref offset);

            HandleAnimation(anim_num, anim_flags, anim_faces);

            return true;
        }

        private bool Parse_drawextinfo(byte[] packet, ref int offset)
        {
            var flags = (NewClient.NewDrawInfo)BufferTokenizer.GetStringAsInt(packet, ref offset);
            var message_type = (NewClient.MsgTypes)BufferTokenizer.GetStringAsInt(packet, ref offset);
            var sub_type = BufferTokenizer.GetStringAsInt(packet, ref offset);
            var message = BufferTokenizer.GetRemainingBytesAsString(packet, ref offset);

            HandleDrawExtInfo(flags, message_type, sub_type, message);

            return true;
        }

        private bool Parse_image2(byte[] packet, ref int offset)
        {
            var image_face = BufferTokenizer.GetUInt32(packet, ref offset);
            var image_faceset = BufferTokenizer.GetByte(packet, ref offset);
            var image_len = BufferTokenizer.GetUInt32(packet, ref offset);
            var image_png = BufferTokenizer.GetBytes(packet, ref offset, (int)image_len);

            HandleImage2(image_face, image_faceset, image_png);

            return true;
        }

        private bool Parse_magicmap(byte[] packet, ref int offset)
        {
            var mm_width = BufferTokenizer.GetStringAsInt(packet, ref offset);
            var mm_height = BufferTokenizer.GetStringAsInt(packet, ref offset);
            var mm_px = BufferTokenizer.GetStringAsInt(packet, ref offset);
            var mm_py = BufferTokenizer.GetStringAsInt(packet, ref offset);

            var mm_data = BufferTokenizer.GetBytes(packet, ref offset, mm_width * mm_height);

            HandleMagicMap(mm_width, mm_height, mm_px, mm_py, mm_data);

            return true;
        }
    }
}

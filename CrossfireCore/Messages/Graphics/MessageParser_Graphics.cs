using System;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageParser
    {
        protected abstract void HandleAnimation(UInt16 AnimationNumber, UInt16 AnimationFlags,
            UInt16[] AnimationFaces);
        protected abstract void HandleFace2(UInt16 face, byte faceset, UInt32 checksum, string filename);
        protected abstract void HandleImage2(UInt32 image_face, byte image_faceset, byte[] image_png);
        protected abstract void HandleMagicMap(int Width, int Height, int PlayerX, int PlayerY, byte[] MapData);

        private void AddGraphicsParsers()
        {
            AddCommandHandler("anim", new CommandParserDefinition(Parse_anim));
            AddCommandHandler("face2", new CommandParserDefinition(Parse_face2));
            AddCommandHandler("image2", new CommandParserDefinition(Parse_image2));
            AddCommandHandler("magicmap", new CommandParserDefinition(Parse_magicmap));
        }

        private bool Parse_anim(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var anim_num = BufferTokenizer.GetUInt16(Message, ref DataOffset);
            var anim_flags = BufferTokenizer.GetUInt16(Message, ref DataOffset);
            var anim_faces = new UInt16[(DataEnd - DataOffset) / 2];

            int anim_offset = 0;
            while (DataOffset < DataEnd)
                anim_faces[anim_offset++] = BufferTokenizer.GetUInt16(Message, ref DataOffset);

            HandleAnimation(anim_num, anim_flags, anim_faces);

            return true;
        }

        private bool Parse_face2(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var face = BufferTokenizer.GetUInt16(Message, ref DataOffset);
            var face_faceset = BufferTokenizer.GetByte(Message, ref DataOffset);
            var checksum = BufferTokenizer.GetUInt32(Message, ref DataOffset);
            var filename = BufferTokenizer.GetRemainingBytesAsString(Message, ref DataOffset, DataEnd);

            HandleFace2(face, face_faceset, checksum, filename);

            return true;
        }

        private bool Parse_image2(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var image_face = BufferTokenizer.GetUInt32(Message, ref DataOffset);
            var image_faceset = BufferTokenizer.GetByte(Message, ref DataOffset);
            var image_len = BufferTokenizer.GetUInt32(Message, ref DataOffset);
            var image_png = BufferTokenizer.GetBytes(Message, ref DataOffset, (int)image_len);

            HandleImage2(image_face, image_faceset, image_png);

            return true;
        }

        private bool Parse_magicmap(byte[] Message, ref int DataOffset, int DataEnd)
        {
            var mm_width = BufferTokenizer.GetStringAsInt(Message, ref DataOffset, DataEnd);
            var mm_height = BufferTokenizer.GetStringAsInt(Message, ref DataOffset, DataEnd);
            var mm_px = BufferTokenizer.GetStringAsInt(Message, ref DataOffset, DataEnd);
            var mm_py = BufferTokenizer.GetStringAsInt(Message, ref DataOffset, DataEnd);

            var mm_data = BufferTokenizer.GetBytes(Message, ref DataOffset, mm_width * mm_height);

            HandleMagicMap(mm_width, mm_height, mm_px, mm_py, mm_data);

            return true;
        }
    }
}

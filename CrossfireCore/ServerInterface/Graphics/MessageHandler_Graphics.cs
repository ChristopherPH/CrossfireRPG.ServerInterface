using System;
using System.Collections.Generic;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public partial class MessageHandler
    {
        public event EventHandler<AnimationEventArgs> Animation;
        public event EventHandler<Face2EventArgs> Face2;
        public event EventHandler<Image2EventArgs> Image2;
        public event EventHandler<MagicMapEventArgs> MagicMap;

        protected override void HandleAnimation(ushort AnimationNumber, ushort AnimationFlags, ushort[] AnimationFaces)
        {
            Animation?.Invoke(this, new AnimationEventArgs()
            {
                AnimationNumber = AnimationNumber,
                AnimationFlags = AnimationFlags,
                AnimationFaces = AnimationFaces,
            });
        }

        protected override void HandleFace2(UInt16 face, byte faceset, UInt32 checksum, string filename)
        {
            Face2?.Invoke(this, new Face2EventArgs()
            {
                Face = face,
                FaceSet = faceset,
                Checksum = checksum,
                FileName = filename,
            });
        }

        protected override void HandleImage2(uint image_face, byte image_faceset, byte[] image_png)
        {
            Image2?.Invoke(this, new Image2EventArgs()
            {
                ImageFace = image_face,
                ImageFaceSet = image_faceset,
                ImageData = image_png
            });
        }

        protected override void HandleMagicMap(int Width, int Height, int PlayerX, int PlayerY, byte[] MapData)
        {
            MagicMap?.Invoke(this, new MagicMapEventArgs()
            {
                Width = Width,
                Height = Height,
                PlayerX = PlayerX,
                PlayerY = PlayerY,
                MapData = MapData
            });
        }

        public class AnimationEventArgs : SingleCommandEventArgs
        {
            public ushort AnimationNumber { get; set; }
            public ushort AnimationFlags { get; set; }
            public ushort[] AnimationFaces { get; set; }
        }

        public class Face2EventArgs : SingleCommandEventArgs
        {
            public UInt16 Face { get; set; }
            public byte FaceSet { get; set; }
            public UInt32 Checksum { get; set; }
            public string FileName { get; set; }
        }

        public class Image2EventArgs : SingleCommandEventArgs
        {
            public UInt32 ImageFace { get; set; }
            public byte ImageFaceSet { get; set; }
            public byte[] ImageData { get; set; }
        }

        public class MagicMapEventArgs : SingleCommandEventArgs
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public int PlayerX { get; set; }
            public int PlayerY { get; set; }
            public byte[] MapData { get; set; }
        }
    }
}

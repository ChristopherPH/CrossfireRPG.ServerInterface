using CrossfireCore;
using CrossfireCore.ServerInterface;
using CrossfireCore.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.Managers
{
    public class FaceManager
    {
        public FaceManager(SocketConnection Connection, MessageBuilder Builder, MessageParser Parser)
        {
            _Connection = Connection;
            _Builder = Builder;
            _Parser = Parser;

            _Connection.OnStatusChanged += _Connection_OnStatusChanged;
            _Parser.Image2 += _Parser_Image2;
        }

        private SocketConnection _Connection;
        private MessageBuilder _Builder;
        private MessageParser _Parser;
        static Logger _Logger = new Logger(nameof(FaceManager));

        private Dictionary<UInt32, FaceInfo> _Faces { get; } = new Dictionary<UInt32, FaceInfo>();

        public event EventHandler<FaceAvailableEventArgs> FaceAvailable;

        public Image GetFace(Int32 Face, bool RequestIfMissing = true)
        {
            return GetFace((UInt32)Face, RequestIfMissing);
        }

        public Image GetFace(Int64 Face, bool RequestIfMissing = true)
        {
            return GetFace((UInt32)Face, RequestIfMissing);
        }

        public Image GetFace(string Face, bool RequestIfMissing = true)
        {
            if (!uint.TryParse(Face, out var value))
                return null;

            return GetFace(value, RequestIfMissing);
        }

        public Image GetFace(UInt32 Face, bool RequestIfMissing = true)
        {
            if (Face == 0)
                return null;

            if (!_Faces.TryGetValue(Face, out var faceInfo))
            {
                if (RequestIfMissing)
                {
                    _Logger.Warning("Map missing face {0}, requesting face", Face);
                    _Builder.SendAskFace((int)Face);
                }

                return null;
            }

            return faceInfo.Image;
        }

        private void _Connection_OnStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            _Faces.Clear();
        }

        private void _Parser_Image2(object sender, MessageParserBase.Image2EventArgs e)
        {
            using (var ms = new MemoryStream(e.ImageData))
            {
                Image image = Image.FromStream(ms);

                if (image == null)
                {
                    _Logger.Error("Face has invalid image data " + e.ImageFace);
                    return;
                }

                if (_Faces.ContainsKey(e.ImageFace))
                {
                    _Logger.Error("Face already exists " + e.ImageFace);
                }

                _Faces[e.ImageFace] = new FaceInfo()
                {
                    Image = image,
                    FaceSet = e.ImageFaceSet
                };

                FaceAvailable?.Invoke(this, new FaceAvailableEventArgs()
                {
                    Face = e.ImageFace
                });
            }
        }

        private class FaceInfo
        {
            public Image Image { get; set; }
            public Size ImageSize => Image == null ? Size.Empty : Image.Size;
            public UInt32 Face { get; set; }
            public byte FaceSet { get; set; }
        }
    }

    public class FaceAvailableEventArgs : EventArgs
    {
        public UInt32 Face { get; set; }
    }
}

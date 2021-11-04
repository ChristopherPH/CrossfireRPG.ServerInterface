using Common;
using CrossfireCore.ServerInterface;
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
        private List<UInt32> _MissingFaces = new List<UInt32>();
        private List<KeyValuePair<UInt32, Action<Image>>> _FaceAvailableActions = new List<KeyValuePair<uint, Action<Image>>>();

        public event EventHandler<FaceAvailableEventArgs> FaceAvailable;

        public Image GetFace(Int32 Face, Action<Image> FaceAvailable = null, bool RequestIfMissing = true)
        {
            return GetFace((UInt32)Face, FaceAvailable, RequestIfMissing);
        }

        public Image GetFace(Int64 Face, Action<Image> FaceAvailable = null, bool RequestIfMissing = true)
        {
            return GetFace((UInt32)Face, FaceAvailable, RequestIfMissing);
        }

        public Image GetFace(string Face, Action<Image> FaceAvailable = null, bool RequestIfMissing = true)
        {
            if (!uint.TryParse(Face, out var value))
                return null;

            return GetFace(value, FaceAvailable, RequestIfMissing);
        }

        public Image GetFace(UInt32 Face, Action<Image> FaceAvailable = null, bool RequestIfMissing = true)
        {
            if (Face == 0)
                return null;

            if (!_Faces.TryGetValue(Face, out var faceInfo))
            {
                if ((_Connection.ConnectionStatus == ConnectionStatuses.Connected) &&
                    RequestIfMissing &&
                    !_MissingFaces.Contains(Face))
                {
                    _MissingFaces.Add(Face);

                    //NOTE: the server has a bug where if you connect and don't pick
                    //      a character, the next time you connect faces won't be
                    //      sent, so we see missing knowledge and character portraits
                    //      during testing
                    _Logger.Warning("Missing face {0}, requesting face", Face);
                    _Builder.SendAskFace((int)Face);
                }

                if (FaceAvailable != null)
                {
                    _FaceAvailableActions.Add(new KeyValuePair<uint, Action<Image>>(Face, FaceAvailable));
                }

                return null;
            }

            return faceInfo.Image;
        }

        /// <summary>
        /// Sets the face immediately if available, or sets the face when face is received
        /// </summary>
        public void SetFace(UInt32 Face, Action<Image> FaceAvailable)
        {
            if ((Face == 0) || (FaceAvailable == null))
                return;

            var Image = GetFace(Face, FaceAvailable, true);
            if (Image != null)
                FaceAvailable(Image);
        }

        public void SetFace(Int32 Face, Action<Image> FaceAvailable)
        {
            SetFace((UInt32)Face, FaceAvailable);
        }

        private void _Connection_OnStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            _Faces.Clear();
            _MissingFaces.Clear();
            _FaceAvailableActions.Clear();
        }

        private void _Parser_Image2(object sender, MessageParser.Image2EventArgs e)
        {
            _Logger.Info("Received Face {0}:{1}", e.ImageFace, e.ImageFaceSet);

            var ix = _MissingFaces.IndexOf(e.ImageFace);
            if (ix != -1)
                _MissingFaces.RemoveAt(ix);

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

                //HACK: use ToList() to stop enumerable from being able to be changed while iterating
                foreach (var faceAction in _FaceAvailableActions.Where(x => x.Key == e.ImageFace).ToList())
                    faceAction.Value(image);
                _FaceAvailableActions.RemoveAll(x => x.Key == e.ImageFace);
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

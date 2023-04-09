using Common;
using CrossfireCore.Managers;
using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Crossfire.Managers
{
    public class FaceManager : DataManager
    {
        public FaceManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Handler.Image2 += _Handler_Image2;
            Handler.Face2 += _Handler_Face2;
        }

        static Logger _Logger = new Logger(nameof(FaceManager));

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => false;

        protected override void ClearData()
        {
            lock (_FaceLock)
            {
                _Faces.Clear();
            }

            lock (_MissingLock)
            {
                _MissingFaces.Clear();
            }

            lock (_ActionLock)
            {
                _FaceAvailableActions.Clear();
            }
        }

        private Dictionary<UInt32, FaceInfo> _Faces { get; } = new Dictionary<UInt32, FaceInfo>();
        private List<UInt32> _MissingFaces = new List<UInt32>();
        private List<KeyValuePair<UInt32, Action<Image>>> _FaceAvailableActions = new List<KeyValuePair<uint, Action<Image>>>();
        private object _FaceLock = new object();
        private object _MissingLock = new object();
        private object _ActionLock = new object();

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

            bool hasFace;
            FaceInfo faceInfo;

            lock (_FaceLock)
            {
                hasFace = _Faces.TryGetValue(Face, out faceInfo);
            }

            if (!hasFace)
            {
                if ((Connection.ConnectionStatus == ConnectionStatuses.Connected) &&
                    RequestIfMissing)
                {
                    bool requestedFace = false;

                    lock (_MissingLock)
                    {
                        requestedFace = _MissingFaces.Contains(Face);
                        if (!requestedFace)
                            _MissingFaces.Add(Face);
                    }

                    if (!requestedFace)
                    {
                        //NOTE: the server has a bug where if you connect and don't pick
                        //      a character, the next time you connect faces won't be
                        //      sent, so we see missing knowledge and character portraits
                        //      during testing
                        _Logger.Warning("Missing face {0}, requesting face", Face);
                        Builder.SendAskFace((int)Face);
                    }
                }

                if (FaceAvailable != null)
                {
                    lock (_ActionLock)
                    {
                        _FaceAvailableActions.Add(new KeyValuePair<uint, Action<Image>>(Face, FaceAvailable));
                    }
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

        private void _Handler_Image2(object sender, MessageHandler.Image2EventArgs e)
        {
            _Logger.Info("Received Face {0}:{1}", e.ImageFace, e.ImageFaceSet);

            lock (_MissingLock)
            {
                var ix = _MissingFaces.IndexOf(e.ImageFace);
                if (ix != -1)
                    _MissingFaces.RemoveAt(ix);
            }

            using (var ms = new MemoryStream(e.ImageData))
            {
                Image image = Image.FromStream(ms);

                if (image == null)
                {
                    _Logger.Error("Face has invalid image data " + e.ImageFace);
                    return;
                }

                lock (_FaceLock)
                {
                    if (_Faces.ContainsKey(e.ImageFace))
                    {
                        _Logger.Error("Face already exists " + e.ImageFace);
                    }

                    _Faces[e.ImageFace] = new FaceInfo()
                    {
                        Image = image,
                        FaceSet = e.ImageFaceSet
                    };
                }

                FaceAvailable?.Invoke(this, new FaceAvailableEventArgs()
                {
                    Face = e.ImageFace
                });

                lock (_ActionLock)
                {
                    foreach (var faceAction in _FaceAvailableActions.Where(x => x.Key == e.ImageFace))
                        faceAction.Value(image);
                    _FaceAvailableActions.RemoveAll(x => x.Key == e.ImageFace);
                }
            }
        }

        private void _Handler_Face2(object sender, MessageHandler.Face2EventArgs e)
        {
            //TODO: Implement face cache

            bool requestedFace = false;

            lock (_MissingLock)
            {
                requestedFace = _MissingFaces.Contains(e.Face);
                if (!requestedFace)
                    _MissingFaces.Add(e.Face);
            }

            if (!requestedFace)
            {
                _Logger.Warning($"Face {e.Face} not in cache, requesting face");
                Builder.SendAskFace(e.Face);
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

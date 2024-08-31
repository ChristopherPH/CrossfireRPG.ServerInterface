using Common;
using CrossfireCore.ServerInterface;
using CrossfireRPG.ServerInterface.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossfireCore.Managers.FaceManagement
{
    public abstract class FaceDataManager<TImage> : DataManager where TImage : class
    {
        public FaceDataManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Handler.Image2 += _Handler_Image2;
            Handler.Face2 += _Handler_Face2;
            Handler.Smooth += _Handler_Smooth;
        }

        protected abstract TImage CreateImage(byte[] ImageData);

        public static Logger Logger { get; } = new Logger(nameof(FaceDataManager<TImage>));

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => false;

        protected override void ClearData(bool disconnected)
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

            lock (_SmoothLock)
            {
                _smoothFaces.Clear();
            }
        }

        private Dictionary<UInt32, FaceInfo> _Faces { get; } = new Dictionary<UInt32, FaceInfo>();
        private List<UInt32> _MissingFaces = new List<UInt32>();
        private List<KeyValuePair<UInt32, Action<TImage>>> _FaceAvailableActions = new List<KeyValuePair<uint, Action<TImage>>>();
        private Dictionary<Int32, SmoothInfo> _smoothFaces = new Dictionary<Int32, SmoothInfo>();

        private object _FaceLock = new object();
        private object _MissingLock = new object();
        private object _ActionLock = new object();
        private object _SmoothLock = new object();

        public event EventHandler<FaceAvailableEventArgs> FaceAvailable;

        public TImage GetFace(Int32 Face, Action<TImage> FaceAvailable = null, bool RequestIfMissing = true)
        {
            return GetFace((UInt32)Face, FaceAvailable, RequestIfMissing);
        }

        public TImage GetFace(Int64 Face, Action<TImage> FaceAvailable = null, bool RequestIfMissing = true)
        {
            return GetFace((UInt32)Face, FaceAvailable, RequestIfMissing);
        }

        public TImage GetFace(string Face, Action<TImage> FaceAvailable = null, bool RequestIfMissing = true)
        {
            if (!uint.TryParse(Face, out var value))
                return null;

            return GetFace(value, FaceAvailable, RequestIfMissing);
        }

        public TImage GetFace(UInt32 Face, Action<TImage> FaceAvailable = null, bool RequestIfMissing = true)
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
                        Logger.Warning("Missing face {0}, requesting face", Face);
                        Builder.SendAskFace((int)Face);
                    }
                }

                if (FaceAvailable != null)
                {
                    lock (_ActionLock)
                    {
                        _FaceAvailableActions.Add(new KeyValuePair<uint, Action<TImage>>(Face, FaceAvailable));
                    }
                }

                return null;
            }

            return faceInfo.Image;
        }

        /// <summary>
        /// Sets the face immediately if available, or sets the face when face is received
        /// </summary>
        public void SetFace(UInt32 Face, Action<TImage> FaceAvailable)
        {
            if ((Face == 0) || (FaceAvailable == null))
                return;

            var T = GetFace(Face, FaceAvailable, true);
            if (T != null)
                FaceAvailable(T);
        }

        public void SetFace(Int32 Face, Action<TImage> FaceAvailable)
        {
            SetFace((UInt32)Face, FaceAvailable);
        }

        private void _Handler_Image2(object sender, MessageHandler.Image2EventArgs e)
        {
            Logger.Info("Received Face {0}:{1}", e.ImageFace, e.ImageFaceSet);

            lock (_MissingLock)
            {
                var ix = _MissingFaces.IndexOf(e.ImageFace);
                if (ix != -1)
                    _MissingFaces.RemoveAt(ix);
            }

            var image = CreateImage(e.ImageData);
            if (image == null)
            {
                Logger.Error("Failed to create face {0}:{1}", e.ImageFace, e.ImageFaceSet);
                return;
            }

            lock (_FaceLock)
            {
                if (_Faces.ContainsKey(e.ImageFace))
                {
                    Logger.Error("Face already exists " + e.ImageFace);
                }

                _Faces[e.ImageFace] = new FaceInfo()
                {
                    Image = image,
                    Face = e.ImageFace,
                    FaceSet = e.ImageFaceSet,
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
                Logger.Warning($"Face {e.Face} not in cache, requesting face");
                Builder.SendAskFace(e.Face);
            }
        }


        public Int32 GetSmoothFace(Int32 SmoothFace)
        {
            if (SmoothFace == 0)
                return 0;

            bool found;
            SmoothInfo info;

            lock (_SmoothLock)
                found = _smoothFaces.TryGetValue(SmoothFace, out info);

            //TODO: Implement AskSmooth() if not found, and user wants to
            //      request missing smoothing

            if (!found)
                return 0;

            return info.SmoothFace;
        }

        private void _Handler_Smooth(object sender, MessageHandler.SmoothEventArgs e)
        {
            lock (_SmoothLock)
            {
                _smoothFaces[e.Smooth] = new SmoothInfo()
                {
                    Face = e.Smooth,
                    SmoothFace = e.SmoothFace,
                };
            }
        }

        private class FaceInfo
        {
            public TImage Image { get; set; }
            public UInt32 Face { get; set; }
            public byte FaceSet { get; set; }
        }

        private class SmoothInfo
        {
            /// <summary>
            /// Face to Smooth
            /// </summary>
            public Int32 Face { get; set; }

            /// <summary>
            /// Face to use when smoothing
            /// </summary>
            public Int32 SmoothFace { get; set; }
        }
    }

    public class FaceAvailableEventArgs : EventArgs
    {
        public UInt32 Face { get; set; }
    }
}

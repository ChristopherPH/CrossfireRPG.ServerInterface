/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using Common.Logging;
using CrossfireRPG.ServerInterface.Network;
using CrossfireRPG.ServerInterface.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CrossfireRPG.ServerInterface.Managers.FaceManagement
{
    /// <summary>
    /// Manager to handle face, image, and smoothing data
    /// </summary>
    /// <remarks>This class is abstract and requires derived implementations to define how images
    /// are created and destroyed.</remarks>
    /// <typeparam name="TImage">The type representing an image associated with a face.</typeparam>
    public abstract class FaceDataManager<TImage> : DataManager where TImage : class
    {
        public FaceDataManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Handler.Image2 += _Handler_Image2;
            Handler.Face2 += _Handler_Face2;
            Handler.Smooth += _Handler_Smooth;
        }

        /// <summary>
        /// Creates an image object from the provided binary image data.
        /// </summary>
        /// <param name="ImageData">The binary data representing the image. Cannot be null or empty.</param>
        /// <returns>An instance of <typeparamref name="TImage"/> representing the created image.</returns>
        protected abstract TImage CreateImage(byte[] ImageData);

        /// <summary>
        /// Releases resources associated with the specified image.
        /// </summary>
        /// <remarks>This method is intended to clean up resources tied to the provided image.
        /// Implementations should ensure that all associated resources are properly released.</remarks>
        /// <param name="image">The image to be destroyed. Must not be null.</param>
        protected abstract void DestroyImage(TImage image);

        public static Logger Logger { get; } = new Logger(nameof(FaceDataManager<TImage>));

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => false;

        protected override void ClearData(bool disconnected)
        {
            lock (_FaceLock)
            {
                for (int ix = 0; ix < _Faces.Length; ix++)
                {
                    if (_Faces[ix] == null)
                        continue;

                    if (_Faces[ix].Image != null)
                        DestroyImage(_Faces[ix].Image);

                    _Faces[ix] = null;
                }
            }

            lock (_RequestedFaces)
            {
                _RequestedFaces.Clear();
            }

            lock (_FaceAvailableActions)
            {
                _FaceAvailableActions.Clear();
            }

            lock (_SmoothFaces)
            {
                _SmoothFaces = new UInt32[_SmoothFaces.Length];
            }
        }

        /// <summary>
        /// Initial size of faces array
        /// </summary>
        /// <remarks>Note that face max is currently a uint16 / 0xFFFF,
        /// as determined by the protocol and internal face storage.
        /// As of this writing, the face max is around 6750</remarks>
        const int InitialFaceSize = 8192;

        /// <summary>
        /// Face Data
        /// </summary>
        private FaceData[] _Faces = new FaceData[InitialFaceSize];

        /// <summary>
        /// Smooth Data
        /// </summary>
        private UInt32[] _SmoothFaces = new UInt32[InitialFaceSize];

        /// <summary>
        /// Contains faces that have been requested but not yet received
        /// </summary>
        private HashSet<UInt32> _RequestedFaces = new HashSet<UInt32>();

        /// <summary>
        /// Contains actions to run when a face becomes available
        /// </summary>
        private Dictionary<UInt32, List<Action<TImage>>> _FaceAvailableActions = new Dictionary<UInt32, List<Action<TImage>>>();


        //TODO: Consider using ReaderWriterLockSlim
        private object _FaceLock = new object();

        /// <summary>
        /// Raised when a face becomes available
        /// </summary>
        public event EventHandler<FaceAvailableEventArgs> FaceAvailable;


        public bool ContainsFace(UInt32 Face)
        {
            if (Face == 0)
                return false;

            lock (_FaceLock)
            {
                return (Face < _Faces.Length) && (_Faces[Face] != null);
            }
        }

        public bool AskFace(UInt32 Face)
        {
            if (Face == 0)
                return false;

            lock (_RequestedFaces)
            {
                _RequestedFaces.Add(Face);
            }

            return Builder.SendProtocolAskFace(Face);
        }

        /// <summary>
        /// Gets the image associated with a face
        /// </summary>
        /// <param name="Face">Face Number</param>
        /// <param name="FaceAvailable">Action to run when the face becomes available in the face manager</param>
        /// <param name="RequestIfMissing">Flag to auto-request the face image if the face manager does not contain the face</param>
        /// <returns>Image, or null if the face manager does not contain the face</returns>
        public TImage GetFace(UInt32 Face, Action<TImage> FaceAvailable = null, bool RequestIfMissing = true)
        {
            //0 is not a valid face number
            if (Face == 0)
                return null;

            FaceData faceData = null;

            //Retrieve the requested face data if available
            lock (_FaceLock)
            {
                if (Face < _Faces.Length)
                    faceData = _Faces[Face];
            }

            //Face data exists, return image
            if (faceData != null)
                return faceData.Image;

            //Request missing face if asked
            if ((Connection.ConnectionStatus == ConnectionStatuses.Connected) &&
                RequestIfMissing)
            {
                bool requestFace = false;

                //Add face to requested faces
                lock (_RequestedFaces)
                {
                    requestFace = _RequestedFaces.Add(Face);
                }

                if (requestFace)
                {
                    //NOTE: the server has a bug where if you connect and don't pick
                    //      a character, the next time you connect faces won't be
                    //      sent, so we see missing knowledge and character portraits
                    //      during testing
                    Logger.Warning("Missing face {0}, requesting face", Face);
                    Builder.SendProtocolAskFace(Face);
                }
            }

            //Add action to run when face becomes available
            if (FaceAvailable != null)
            {
                lock (_FaceAvailableActions)
                {
                    if (_FaceAvailableActions.TryGetValue(Face, out var actions))
                    {
                        //Add to existing actions
                        actions.Add(FaceAvailable);
                    }
                    else
                    {
                        //Create new action list
                        _FaceAvailableActions[Face] = new List<Action<TImage>>()
                        {
                            FaceAvailable
                        };
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the face immediately if available, or sets the face when face is received
        /// </summary>
        public void SetFace(UInt32 Face, Action<TImage> FaceAvailable)
        {
            if ((Face == 0) || (FaceAvailable == null))
                return;

            var img = GetFace(Face, FaceAvailable, true);
            if (img != null)
                FaceAvailable(img);
        }

        private void _Handler_Image2(object sender, MessageHandler.Image2EventArgs e)
        {
            Logger.Info("Received Face {0}:{1}", e.ImageFace, e.ImageFaceSet);

            //Remove face from requested faces
            lock (_RequestedFaces)
            {
                _RequestedFaces.Remove(e.ImageFace);
            }

            //Create image from face image data
            var image = CreateImage(e.ImageData);
            if (image == null)
            {
                Logger.Error("Failed to create face {0}:{1}", e.ImageFace, e.ImageFaceSet);
                return;
            }

            lock (_FaceLock)
            {
                if ((e.ImageFace < _Faces.Length) && (_Faces[e.ImageFace] != null))
                {
                    Logger.Error("Face already exists " + e.ImageFace);
                }

                //Resize the array if needed
                if (e.ImageFace >= _Faces.Length)
                {
                    var newSize = _Faces.Length;

                    //Double the size until it fits
                    while (e.ImageFace >= newSize)
                        newSize *= 2;

                    Array.Resize(ref _Faces, newSize);
                }

                //Add the face data
                _Faces[e.ImageFace] = new FaceData()
                {
                    Image = image,
                    ImageData = e.ImageData,
                    FaceSet = e.ImageFaceSet,
                };
            }

            //Raise the face available event
            FaceAvailable?.Invoke(this, new FaceAvailableEventArgs()
            {
                Face = e.ImageFace
            });


            //Run all actions associated with this face
            lock (_FaceAvailableActions)
            {
                if (_FaceAvailableActions.TryGetValue(e.ImageFace, out var actions))
                {
                    foreach (var action in actions)
                    {
                        action(image);
                    }

                    _FaceAvailableActions.Remove(e.ImageFace);
                }
            }
        }

        private void _Handler_Face2(object sender, MessageHandler.Face2EventArgs e)
        {
            //TODO: Implement face cache

            bool requestFace = false;

            lock (_RequestedFaces)
            {
                requestFace = _RequestedFaces.Add(e.Face);
            }

            if (requestFace)
            {
                Logger.Warning($"Face {e.Face} not in cache, requesting face");
                Builder.SendProtocolAskFace(e.Face);
            }
        }


        public UInt32 GetSmoothFace(UInt32 SmoothFace)
        {
            if (SmoothFace == 0)
                return 0;

            //TODO: Implement AskSmooth() if not found, and user wants to
            //      request missing smoothing

            lock (_SmoothFaces)
            {
                if (SmoothFace < _SmoothFaces.Length)
                    return _SmoothFaces[SmoothFace];
            }

            return 0;
        }

        private void _Handler_Smooth(object sender, MessageHandler.SmoothEventArgs e)
        {
            lock (_SmoothFaces)
            {
                //Resize the array if needed
                if (e.Smooth >= _SmoothFaces.Length)
                {
                    var newSize = _SmoothFaces.Length;

                    //Double the size until it fits
                    while (e.Smooth >= newSize)
                        newSize *= 2;

                    Array.Resize(ref _SmoothFaces, newSize);
                }

                //Set the smooth face data
                _SmoothFaces[e.Smooth] = e.SmoothFace;
            }
        }

        private class FaceData
        {
            /// <summary>
            /// FaceSet of Face
            /// </summary>
            public byte FaceSet { get; set; }

            /// <summary>
            /// Raw data of face image
            /// </summary>
            public byte[] ImageData { get; set; }

            /// <summary>
            /// The image associated with face
            /// </summary>
            public TImage Image { get; set; }
        }
    }

    public class FaceAvailableEventArgs : EventArgs
    {
        public UInt32 Face { get; set; }
    }
}

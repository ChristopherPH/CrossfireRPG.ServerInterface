using Common;
using CrossfireCore.Managers;
using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.Managers
{
    public class MapSizeManager : Manager
    {
        public MapSizeManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Connection.OnStatusChanged += _Connection_OnStatusChanged;

            Handler.Setup += Handler_Setup;
        }

        static Logger _Logger = new Logger(nameof(MapSizeManager));

        public event EventHandler<MapSizeEventArgs> MapSizeChanged;

        protected void OnMapSizeChanged(MapSizeEventArgs args)
        {
            MapSizeChanged?.Invoke(this, args);
        }

        private void Handler_Setup(object sender, MessageHandler.SetupEventArgs e)
        {
            switch (e.SetupCommand.ToLower())
            {
                case "mapsize":
                    HandleMapSize(e.SetupValue);
                    break;
            }
        }

        private object _QueueLock = new object();

        private void _Connection_OnStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            if (e.Status == ConnectionStatuses.Disconnected)
            {
                lock (_QueueLock)
                {
                    _RequestedMapSizes.Clear();
                }

                MaximumMapWidth = 0;
                MaximumMapHeight = 0;
                CurrentMapWidth = 0;
                CurrentMapHeight = 0;
            }
        }

        /// <summary>
        /// Default map width for clients that don't support the [setup mapsize] command
        /// (generally 11 unless server has been modified)
        /// </summary>
        [DefaultValue(11)]
        public int DefaultMapWidth { get; set; } = 11;

        /// <summary>
        /// Default map height for clients that don't support the [setup mapsize] command
        /// (generally 11 unless server has been modified)
        /// </summary>
        [DefaultValue(11)]
        public int DefaultMapHeight { get; set; } = 11;

        /// <summary>
        /// Maximum map width the server supports
        /// (generally 25 unless server has been modified)
        /// </summary>
        [DefaultValue(25)]
        public int MaximumMapWidth { get; private set; } = 0;

        /// <summary>
        /// Maximum map height the server supports
        /// (generally 25 unless server has been modified)
        /// </summary>
        [DefaultValue(25)]
        public int MaximumMapHeight { get; private set; } = 0;

        /// <summary>
        /// Minimum map width the server supports
        /// (generally 9 unless server has been modified)
        /// </summary>
        [DefaultValue(9)]
        public int MinimumMapWidth { get; set; } = 9;

        /// <summary>
        /// Minimum map height the server supports
        /// (generally 9 unless server has been modified)
        /// </summary>
        [DefaultValue(9)]
        public int MinimumMapHeight { get; set; } = 9;

        /// <summary>
        /// Current Map Width
        /// </summary>
        public int CurrentMapWidth { get; private set; } = 0;

        /// <summary>
        /// Current Map Height
        /// </summary>
        public int CurrentMapHeight { get; private set; } = 0;

        private int WantedWidth;
        private int WantedHeight;
        Queue<Size> _RequestedMapSizes = new Queue<Size>();

        /// <summary>
        /// Call this to setup the map size once the Version command has been processed
        /// </summary>
        public void SetupMapSize()
        {
            //MapSize command is not valid for this server, so set the map to the server
            //hardcoded default
            if (Handler.ServerProtocolVersion < MessageParser.ServerProtocolVersionSetupCommand)
            {
                SetMapSizeToDefault();

                return;
            }

            //Request the maximum map size
            SetMapSizeInternal(0, 0);

            if ((WantedWidth > 0) && (WantedHeight > 0))
                SetMapSizeInternal(WantedWidth, WantedHeight);
        }

        /// <summary>
        /// Sets the Map Size to the given Width/Height
        /// </summary>
        public bool SetMapSize(Size mapsize)
        {
            return SetMapSize(mapsize.Width, mapsize.Height);
        }

        /// <summary>
        /// Sets the Map Size to the given Width/Height
        /// </summary>
        public bool SetMapSize(int width, int height)
        {
            //Save the map size values in case SetMapSize is called and we are not connected.
            //When SetupMapSize() is called, that will use the saved values to set up the map.
            WantedWidth = width;
            WantedHeight = height;

            _Logger.Info("Wanted mapsize is {0}x{1}", width, height);

            //Setup server: Note that older servers don't understand setup command
            //              and if we aren't connected, this will be 0
            if (Handler.ServerProtocolVersion < MessageParser.ServerProtocolVersionSetupCommand)
            {
                SetMapSizeToDefault();

                return true;
            }

            return SetMapSizeInternal(width, height);
        }

        private bool SetMapSizeInternal(int width, int height)
        {
            //Send the mapsize command
            if (!Builder.SendSetup("mapsize", string.Format("{0}x{1}", width, height)))
                return false;

            //If we successfully sent the command, save the size to match up with the
            //response
            lock (_QueueLock)
            {
                _RequestedMapSizes.Enqueue(new Size(width, height));
            }
            return true;
        }

        private void SetMapSizeToDefault()
        {
            if ((CurrentMapWidth != DefaultMapWidth) || (CurrentMapHeight != DefaultMapHeight))
            {
                CurrentMapWidth = DefaultMapWidth;
                CurrentMapHeight = DefaultMapHeight;

                _Logger.Info("Set current mapsize to default ({0}x{1})", CurrentMapWidth, CurrentMapHeight);

                OnMapSizeChanged(new MapSizeEventArgs() { MapSize = new Size(CurrentMapWidth, CurrentMapHeight) });
            }
        }

        private bool HandleMapSize(string mapsize)
        {
            Size RequestedMapSize;

            lock (_QueueLock)
            {
                if (_RequestedMapSizes.Count == 0)
                    return false;

                RequestedMapSize = _RequestedMapSizes.Dequeue();
            }

            //Check for setup command failure
            if (string.IsNullOrWhiteSpace(mapsize) || (mapsize == "FALSE"))
            {
                //technically we know the servers default is 11, but that is never sent by the server
                _Logger.Warning("Setup: mapsize setup command failed, assume mapsize is fixed at 11x11");
                SetMapSizeToDefault();
                return false;
            }

            //match pattern of (number)x(number) with allowed whitespace and case insensitive x
            var match = System.Text.RegularExpressions.Regex.Match(mapsize, @"^\s*(\d+)\s*[xX]\s*(\d+)\s*$");
            if (!match.Success)
            {
                _Logger.Warning("Setup: mapsize setup response is invalid, setting mapsize to 11x11: {0}", mapsize);
                SetMapSizeToDefault();
                return false;
            }

            //we know we've captured digits so we can assume int.Parse() won't fail here, hence no error checking
            var serverWidth = int.Parse(match.Groups[1].Value);
            var serverHeight = int.Parse(match.Groups[2].Value);

            //We've called "mapsize" with 0x0, in this case the server doesn't set
            //the map value, but instead returns the maximum map size
            if ((RequestedMapSize.Width == 0) && (RequestedMapSize.Height == 0))
            {
                MaximumMapWidth = serverWidth;
                MaximumMapHeight = serverHeight;

                _Logger.Info("Setup: Maximum mapsize is {0}x{1}", MaximumMapWidth, MaximumMapHeight);
                return true;
            }

            //If the map size that was requested matches the response, then update the mapsize
            if ((RequestedMapSize.Width == serverWidth) && (RequestedMapSize.Height == serverHeight))
            {
                CurrentMapWidth = serverWidth;
                CurrentMapHeight = serverHeight;

                _Logger.Info("Setup: Set current mapsize to {0}x{1}", CurrentMapWidth, CurrentMapHeight);

                OnMapSizeChanged(new MapSizeEventArgs() { MapSize = new Size(CurrentMapWidth, CurrentMapHeight) });
                return true;
            }

            //If there are other outstanding requests, then we don't want to call SetMapSize() again
            if (_RequestedMapSizes.Count > 0)
            {
                //Logger.Info("Setup: Ignoring response of {0}x{1}", RequestedMapSize.Width, RequestedMapSize.Height);
                return false;
            }

            //User requested bigger than the server supports, so the server sent back
            //the maximum size it can support.
            //Reset the size to what the server can handle and request again
            if ((RequestedMapSize.Width > serverWidth) || (RequestedMapSize.Height > serverHeight))
            {
                _Logger.Warning("Setup: Requested mapsize {0}x{1} not supported, server suggested mapsize {2}x{3}",
                        RequestedMapSize.Width, RequestedMapSize.Height, serverWidth, serverHeight);

                SetMapSize(serverWidth, serverHeight);
            }
            else if ((RequestedMapSize.Width != serverWidth) || (RequestedMapSize.Height != serverHeight))
            {
                //technically we know the servers minimum is 9, but that is never sent by the server
                _Logger.Warning("Setup: Requested mapsize {0}x{1} rejected, server suggested mapsize {2}x{3}",
                    RequestedMapSize.Width, RequestedMapSize.Height, serverWidth, serverHeight);

                //TODO: Fix the server to return minimum sizes, but in the mean time, reset the size to
                //      what the server returned so we are working with something valid
                SetMapSize(serverWidth, serverHeight);
            }

            return true;
        }
    }

    public class MapSizeEventArgs : EventArgs
    {
        public Size MapSize { get; set; }
    }
}

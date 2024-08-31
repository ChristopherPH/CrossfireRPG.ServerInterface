using Common;
using CrossfireRPG.ServerInterface.Definitions;
using CrossfireRPG.ServerInterface.Network;
using CrossfireRPG.ServerInterface.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CrossfireCore.Managers.MapSizeManagement
{
    /// <summary>
    /// Manager to handle changing the map size
    /// </summary>
    public class MapSizeManager : DataManager
    {
        public MapSizeManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Handler.Setup += Handler_Setup;
        }

        public static Logger Logger { get; } = new Logger(nameof(MapSizeManager));

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => false;

        public event EventHandler<MapSizeEventArgs> MapSizeChanged;
        public event EventHandler<MapSizeEventArgs> MaximumMapSizeChanged;

        protected void OnMapSizeChanged(MapSizeEventArgs args)
        {
            MapSizeChanged?.Invoke(this, args);
        }

        protected void OnMaximumMapSizeChanged(MapSizeEventArgs args)
        {
            MaximumMapSizeChanged?.Invoke(this, args);
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

        protected override void ClearData(bool disconnected)
        {
            lock (_QueueLock)
            {
                _RequestedMapSizes.Clear();
            }

            MaximumMapWidth = MaximumMapWidth;
            MaximumMapHeight = MaximumMapHeight;
            CurrentMapWidth = DefaultMapWidth;
            CurrentMapHeight = DefaultMapHeight;
        }

        /// <summary>
        /// Default map width for clients that don't support the [setup mapsize] command
        /// (generally 11 unless server has been modified)
        /// </summary>
        [DefaultValue(Config.MAP_CLIENT_X_DEFAULT)]
        public int DefaultMapWidth { get; set; } = Config.MAP_CLIENT_X_DEFAULT;

        /// <summary>
        /// Default map height for clients that don't support the [setup mapsize] command
        /// (generally 11 unless server has been modified)
        /// </summary>
        [DefaultValue(Config.MAP_CLIENT_Y_DEFAULT)]
        public int DefaultMapHeight { get; set; } = Config.MAP_CLIENT_Y_DEFAULT;

        /// <summary>
        /// Maximum map width the server supports
        /// (generally 25 unless server has been modified)
        /// </summary>
        [DefaultValue(Config.MAP_CLIENT_X)]
        public int MaximumMapWidth { get; private set; } = Config.MAP_CLIENT_X;

        /// <summary>
        /// Maximum map height the server supports
        /// (generally 25 unless server has been modified)
        /// </summary>
        [DefaultValue(Config.MAP_CLIENT_Y)]
        public int MaximumMapHeight { get; private set; } = Config.MAP_CLIENT_Y;

        /// <summary>
        /// Minimum map width the server supports
        /// (generally 9 unless server has been modified)
        /// </summary>
        [DefaultValue(Config.MAP_CLIENT_X_MINIMUM)]
        public int MinimumMapWidth { get; set; } = Config.MAP_CLIENT_X_MINIMUM;

        /// <summary>
        /// Minimum map height the server supports
        /// (generally 9 unless server has been modified)
        /// </summary>
        [DefaultValue(Config.MAP_CLIENT_Y_MINIMUM)]
        public int MinimumMapHeight { get; set; } = Config.MAP_CLIENT_Y_MINIMUM;

        /// <summary>
        /// Current Map Width
        /// </summary>
        public int CurrentMapWidth { get; private set; } = Config.MAP_CLIENT_X_DEFAULT;

        /// <summary>
        /// Current Map Height
        /// </summary>
        public int CurrentMapHeight { get; private set; } = Config.MAP_CLIENT_Y_DEFAULT;

        private int WantedWidth;
        private int WantedHeight;
        private Queue<MapSizeEventArgs> _RequestedMapSizes = new Queue<MapSizeEventArgs>();

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
            Logger.Info("Client requested maximum mapsize");

            SetMapSizeInternal(0, 0);

            if ((WantedWidth > 0) && (WantedHeight > 0))
                SetMapSizeInternal(WantedWidth, WantedHeight);
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

            Logger.Info("Client requested mapsize of {0}x{1}", width, height);

            //Setup server: Note that older servers don't understand setup command
            //              and if we aren't connected, this will be 0
            if (Handler.ServerProtocolVersion < MessageParser.ServerProtocolVersionSetupCommand)
            {
                SetMapSizeToDefault();

                return true;
            }

            if (!SetMapSizeInternal(width, height))
                return false;

            WantedWidth = 0;
            WantedHeight = 0;

            return true;
        }

        private bool SetMapSizeInternal(int width, int height)
        {
            //Send the mapsize command
            if (!Builder.SendSetup("mapsize", $"{width}x{height}"))
                return false;

            //If we successfully sent the command, save the size to match up with the
            //response
            lock (_QueueLock)
            {
                _RequestedMapSizes.Enqueue(new MapSizeEventArgs(width, height));
            }

            return true;
        }

        private void SetMapSizeToDefault()
        {
            if ((CurrentMapWidth != DefaultMapWidth) || (CurrentMapHeight != DefaultMapHeight))
            {
                CurrentMapWidth = DefaultMapWidth;
                CurrentMapHeight = DefaultMapHeight;

                Logger.Info("Set current mapsize to default ({0}x{1})", CurrentMapWidth, CurrentMapHeight);

                OnMapSizeChanged(new MapSizeEventArgs(CurrentMapWidth, CurrentMapHeight));
            }
        }

        private bool HandleMapSize(string mapsize)
        {
            MapSizeEventArgs RequestedMapSize;

            lock (_QueueLock)
            {
                //If no map size was requested, then we've receieved an unsolicited
                //mapsize response, or have more than one MapSizeManager.
                if (_RequestedMapSizes.Count == 0)
                    return false;

                RequestedMapSize = _RequestedMapSizes.Dequeue();
            }

            //Check for setup command failure
            if (string.IsNullOrWhiteSpace(mapsize) || (mapsize == "FALSE"))
            {
                //technically we know the servers default is 11, but that is never sent by the server
                Logger.Warning("Setup: mapsize setup command failed, assume mapsize is fixed at {0}x{1}",
                    DefaultMapWidth, DefaultMapHeight);
                SetMapSizeToDefault();
                return false;
            }

            //match pattern of (number)x(number) with allowed whitespace and case insensitive x
            var match = System.Text.RegularExpressions.Regex.Match(mapsize, @"^\s*(\d+)\s*[xX]\s*(\d+)\s*$");
            if (!match.Success)
            {
                Logger.Warning("Setup: mapsize setup response is invalid, setting mapsize to {0}x{1}: {2}",
                    DefaultMapWidth, DefaultMapHeight, mapsize);
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

                Logger.Info("Setup: Server returned maximum mapsize is {0}x{1}", MaximumMapWidth, MaximumMapHeight);

                OnMaximumMapSizeChanged(new MapSizeEventArgs(MaximumMapWidth, MaximumMapHeight));
                return true;
            }

            //If the map size that was requested matches the response, then update the mapsize
            if ((RequestedMapSize.Width == serverWidth) && (RequestedMapSize.Height == serverHeight))
            {
                CurrentMapWidth = serverWidth;
                CurrentMapHeight = serverHeight;

                Logger.Info("Setup: Server has set current mapsize to {0}x{1}", CurrentMapWidth, CurrentMapHeight);

                OnMapSizeChanged(RequestedMapSize);
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
                Logger.Warning("Setup: Requested mapsize {0}x{1} not supported, server suggested mapsize {2}x{3}",
                        RequestedMapSize.Width, RequestedMapSize.Height, serverWidth, serverHeight);

                SetMapSize(serverWidth, serverHeight);
            }
            else if ((RequestedMapSize.Width != serverWidth) || (RequestedMapSize.Height != serverHeight))
            {
                //technically we know the servers minimum is 9, but that is never sent by the server
                Logger.Warning("Setup: Requested mapsize {0}x{1} rejected, server suggested mapsize {2}x{3}",
                    RequestedMapSize.Width, RequestedMapSize.Height, serverWidth, serverHeight);

                //Reset the size to what the server returned so we are working with something valid
                SetMapSize(serverWidth, serverHeight);
            }

            return true;
        }
    }

    public class MapSizeEventArgs : EventArgs
    {
        public MapSizeEventArgs(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
        }

        /// <summary>
        /// Width of Map
        /// </summary>
        public int Width { get; private set; } = 0;

        /// <summary>
        /// Height of Map
        /// </summary>
        public int Height { get; private set; } = 0;
    }
}

using Common;
using CrossfireCore.Managers.AnimationManagement;
using CrossfireCore.Managers.MapSizeManagement;
using CrossfireCore.ServerConfig;
using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;

namespace CrossfireCore.Managers.MapManagement
{
    public class MapManager : DataObjectManager<MapObject>
    {
        public MapManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler,
            AnimationDataManager animationManager)
            : base(Connection, Builder, Handler)
        {
            //Individual events
            Handler.Setup += Handler_Setup;
            Handler.NewMap += Handler_NewMap;
            Handler.Player += Handler_Player;
            Handler.Smooth += Handler_Smooth;
            Handler.Tick += Handler_Tick;

            //All part of map2 events
            Handler.MapBegin += Handler_MapBegin;
            Handler.MapEnd += Handler_MapEnd;
            Handler.MapBeginLocation += Handler_MapBeginLocation;
            Handler.MapEndLocation += Handler_MapEndLocation;
            Handler.MapFace += Handler_MapFace;
            Handler.MapAnimation += Handler_MapAnimation;
            Handler.MapDarkness += Handler_MapDarkness;
            Handler.MapClear += Handler_MapClear;
            Handler.MapClearLayer += Handler_MapClearLayer;
            Handler.MapScroll += Handler_MapScroll;

            //Other managers
            AnimationManager = animationManager;
        }

        public static Logger Logger { get; } = new Logger(nameof(MapManager));
        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => false;
        public override DataModificationTypes SupportedModificationTypes =>
            DataModificationTypes.MultiCommandStart | DataModificationTypes.MultiCommandEnd |
            DataModificationTypes.Added |   //Raised after a map has been populated for the first time
            DataModificationTypes.Updated | //Raised when a map has been updated
            DataModificationTypes.Cleared;  //Raised when a map has been cleared

        public AnimationDataManager AnimationManager { get; }

        /// <summary>
        /// Managed Map Object
        /// </summary>
        public MapObject MapObject { get; } = new MapObject();

        /// <summary>
        /// Raised before the existing map is cleared
        /// </summary>
        public event EventHandler<DataUpdatedEventArgs<MapObject>> BeforeMapClear;

        /// <summary>
        /// Raised when a map cell has been updated
        /// </summary>
        public event EventHandler<MapCellUpdatedEventArgs> MapCellUpdated;

        /// <summary>
        /// Raised when the map has been updated : Occurs when ModificationTypes.Updated
        /// is raised, but this event contains additional data
        /// </summary>
        public event EventHandler<MapUpdatedEventArgs> MapUpdated;

        //Private variables
        object _mapDataLock = new object();
        bool _populatingNewMap = false;
        int _mapScrollX = 0;
        int _mapScrollY = 0;
        int CurrentMapWidth = Config.MAP_CLIENT_X_DEFAULT;
        int CurrentMapHeight = Config.MAP_CLIENT_Y_DEFAULT;
        List<MessageHandler.SmoothEventArgs> _smoothFaces = new List<MessageHandler.SmoothEventArgs>();

        //Private variables for creating map updated args
        MapUpdatedEventArgs workingUpdateArgs = new MapUpdatedEventArgs();
        bool workingIsEmpty;
        int workingMinX;
        int workingMaxX;
        int workingMinY;
        int workingMaxY;

        protected override void ClearData(bool disconnected)
        {
            OnBeforeMapClear();

            //Items to clear on new player
            lock (_mapDataLock)
                MapObject.ClearMap();

            _mapScrollX = _mapScrollY = 0;

            //Items to clear on server disconnect
            CurrentMapWidth = Config.MAP_CLIENT_X_DEFAULT;
            CurrentMapHeight = Config.MAP_CLIENT_Y_DEFAULT;
            _smoothFaces.Clear();

            OnDataChanged(DataModificationTypes.Cleared, MapObject);
        }

        private void Handler_Player(object sender, MessageHandler.PlayerEventArgs e)
        {
            //New player
            if (e.tag == 0)
            {
                OnBeforeMapClear();

                //Items to clear on new player
                lock (_mapDataLock)
                {
                    MapObject.ClearMap();
                    MapObject.SetViewportSize(CurrentMapWidth, CurrentMapHeight);
                }

                _mapScrollX = _mapScrollY = 0;

                OnDataChanged(DataModificationTypes.Cleared, MapObject);
            }
        }

        private void Handler_Setup(object sender, MessageHandler.SetupEventArgs e)
        {
            if (e.SetupCommand.ToLower() != "mapsize")
                return;

            if (!MapSizeManager.ParseMapSize(e.SetupValue, out var width, out var height))
            {
                CurrentMapWidth = Config.MAP_CLIENT_X_DEFAULT;
                CurrentMapHeight = Config.MAP_CLIENT_Y_DEFAULT;
                return;
            }

            CurrentMapWidth = width;
            CurrentMapHeight = height;

            //As it turns out, changing the viewport (mapsize) triggers
            //the newmap command before returning the setup command,
            //so we need to adjust the viewport after the map has been
            //created
            MapObject.SetViewportSize(CurrentMapWidth, CurrentMapHeight);
        }

        private void Handler_MapBegin(object sender, System.EventArgs e)
        {
            workingUpdateArgs = new MapUpdatedEventArgs();
            workingIsEmpty = MapObject.IsEmpty;
            workingMinX = MapObject.MinX;
            workingMaxX = MapObject.MaxX;
            workingMinY = MapObject.MinY;
            workingMaxY = MapObject.MaxY;

            //Got a map2 command
            StartMultiCommand();
        }

        private void Handler_MapEnd(object sender, System.EventArgs e)
        {
            //Finished map2 command
            EndMultiCommand();

            //If this is the end of the first map2 after
            //a newmap, then notify map was added
            if (_populatingNewMap)
            {
                _populatingNewMap = false;

                //Notify a new map was added
                OnDataChanged(DataModificationTypes.Added, MapObject);
            }

            //Notify map was updated
            //_mapScrollUpdatedMap check is bad, if true, then _updatedLocations.Count will always be > 0
            if ((workingUpdateArgs.CellLocations.Count > 0) ||
                workingUpdateArgs.MapScrolled)
            {
                //set base properties, other MapUpdatedEventArgs
                //have been updated
                workingUpdateArgs.Modification = DataModificationTypes.Updated;
                workingUpdateArgs.Data = MapObject;
                workingUpdateArgs.UpdatedProperties = null;

                if ((workingIsEmpty != MapObject.IsEmpty) ||
                    (workingMinX != MapObject.MinX) ||
                    (workingMaxX != MapObject.MaxX) ||
                    (workingMinY != MapObject.MinY) ||
                    (workingMaxY != MapObject.MaxY))
                {
                    workingUpdateArgs.MapSizeChanged = true;
                }

                OnDataChanged(workingUpdateArgs);
                OnMapUpdated(workingUpdateArgs);
            }

            workingUpdateArgs = new MapUpdatedEventArgs();
        }

        private void Handler_MapBeginLocation(object sender, MessageHandler.MapLocationEventArgs e)
        {
            //Inside map2 command, updating a single cell
            var worldX = e.X + _mapScrollX;
            var worldY = e.Y + _mapScrollY;

            lock (_mapDataLock)
            {
                //If there is an existing cell, mark it as not yet updated.
                var cell = MapObject.GetCell(worldX, worldY);
                if (cell != null)
                    cell.Updated = false;
            }
        }

        private void Handler_MapEndLocation(object sender, MessageHandler.MapLocationEventArgs e)
        {
            //Finished updating a single cell
            var worldX = e.X + _mapScrollX;
            var worldY = e.Y + _mapScrollY;

            lock (_mapDataLock)
            {
                //If there is an existing cell, and it was updated, notify listeners
                var cell = MapObject.GetCell(worldX, worldY);
                if ((cell != null) && cell.Updated)
                {
                    OnMapCellUpdated(cell);
                    workingUpdateArgs.CellLocations.Add(new MapCellLocation(worldX, worldY));
                    workingUpdateArgs.InsideViewportChanged = true;
                    cell.Updated = false;
                }
            }
        }

        private void Handler_NewMap(object sender, EventArgs e)
        {
            OnBeforeMapClear();

            //Items to clear on new map
            lock (_mapDataLock)
            {
                MapObject.ClearMap();
                MapObject.SetViewportSize(CurrentMapWidth, CurrentMapHeight);
            }

            _mapScrollX = _mapScrollY = 0;

            //Mark that next map population is the first display
            //of this map
            _populatingNewMap = true;

            OnDataChanged(DataModificationTypes.Cleared, MapObject);
        }

        private void Handler_MapFace(object sender, MessageHandler.MapFaceEventArgs e)
        {
            var worldX = e.X + _mapScrollX;
            var worldY = e.Y + _mapScrollY;

            var OutOfBounds = (e.X < 0) || (e.Y < 0) ||
                (e.X >= CurrentMapWidth) || (e.Y >= CurrentMapHeight);

            lock (_mapDataLock)
            {
                //Get pre-existing cell at x/y
                var cell = MapObject.GetCell(worldX, worldY);
                if (cell != null)
                {
                    //if the pre-existing cell is invisible, it means
                    //that it has gone out of view.
                    //Since we've now gotten a face, so we need to clear
                    //the cell data like the server would so we can start
                    //the tile fresh.
                    //However, if the cell was originally out of bounds,
                    //then the server would not have ever cleared the tile
                    //so we can ignore this.
                    if (!cell.Visible && !OutOfBounds)
                    {
                        cell.ClearDarkness();
                        cell.ClearLayers();
                        cell.Updated = true;
                    }
                }
                else //cell doesn't exist, create a new cell
                {
                    cell = new MapCell()
                    {
                        WorldX = worldX,
                        WorldY = worldY,
                    };

                    MapObject.SetCell(cell);
                }

                //add or set face layer
                var layer = new MapLayer()
                {
                    Face = e.Face,
                    SmoothLevel = e.Smooth,
                };

                if (layer.Face == 0)
                {
                    Logger.Warning($"Map: Cell {worldX}/{worldY} layer " +
                        $"{e.Layer} updated from face {cell.Layers[e.Layer].Face} to {layer.Face}");
                }

                if (cell.Layers[e.Layer] != layer)
                {
                    //_Logger.Warning($"Map: Cell {worldX}/{worldY} layer {e.Layer} updated from face {cell.Layers[e.Layer].Face} to {layer.Face}");
                    cell.Layers[e.Layer] = layer;
                    cell.Updated = true;
                }

                //If the face is out of the visible map area, mark it as
                //out of bounds, and note the server doesn't send a clear command
                //for this data.
                cell.OutOfBounds = OutOfBounds;
                cell.Visible = true;
            }
        }

        private void Handler_MapAnimation(object sender, MessageHandler.MapAnimationEventArgs e)
        {
            var worldX = e.X + _mapScrollX;
            var worldY = e.Y + _mapScrollY;

            var OutOfBounds = (e.X < 0) || (e.Y < 0) ||
                (e.X >= CurrentMapWidth) || (e.Y >= CurrentMapHeight);

            lock (_mapDataLock)
            {
                //Get pre-existing cell at x/y
                var cell = MapObject.GetCell(worldX, worldY);
                if (cell != null)
                {
                    //if the pre-existing cell is invisible, it means
                    //that it had gone out of view.
                    //Since we've now gotten a face, so we need to clear
                    //the cell data like the server expects, so we can start
                    //the cell fresh.
                    //However, if the cell was originally out of bounds,
                    //then the server would not have ever cleared the tile
                    //so we can ignore this. (The server doesn't remember
                    //when it sends OOB data so it doesn't ever clear it.)
                    if (!cell.Visible && !OutOfBounds)
                    {
                        cell.ClearDarkness();
                        cell.ClearLayers();
                    }
                }
                else //cell doesn't exist, create a new cell
                {
                    cell = new MapCell()
                    {
                        WorldX = worldX,
                        WorldY = worldY,
                    };

                    MapObject.SetCell(cell);
                }

                //add or set animation layer
                var layer = new MapLayer()
                {
                    SmoothLevel = e.Smooth,
                    Animation = e.Animation,
                    AnimationType = e.AnimationType,
                    AnimationSpeed = e.AnimationSpeed,
                };

                layer.AnimationState.SetAnimation(layer.AnimationType, layer.AnimationSpeed,
                    AnimationManager?.GetAnimationFrameCount(layer.Animation) ?? 0);

                if (cell.Layers[e.Layer] != layer)
                {
                    //_Logger.Warning($"Map: Cell {worldX}/{worldY} layer {e.Layer} updated from face {cell.Layers[e.Layer].Face} to {layer.Face}");
                    cell.Layers[e.Layer] = layer;
                    cell.Updated = true;
                }

                //If the face is out of the visible map area, mark it as
                //out of bounds, and note the server doesn't send a clear command
                //for this data.
                cell.OutOfBounds = OutOfBounds;
                cell.Visible = true;
            }
        }

        private void Handler_MapDarkness(object sender, MessageHandler.MapDarknessEventArgs e)
        {
            var worldX = e.X + _mapScrollX;
            var worldY = e.Y + _mapScrollY;

            lock (_mapDataLock)
            {
                //Get pre-existing cell at x/y
                var cell = MapObject.GetCell(worldX, worldY);
                if (cell != null)
                {
                    //if the pre-existing cell is invisible, it means
                    //that it has gone out of view. However, we've now
                    //gotten a face, so we need to clear the cell data
                    //so we can start fresh
                    if (!cell.Visible)
                    {
                        cell.ClearDarkness();
                        cell.ClearLayers();
                        cell.Updated = true;
                    }
                }
                else //cell doesn't exist, create a new cell
                {
                    cell = new MapCell()
                    {
                        WorldX = worldX,
                        WorldY = worldY,
                    };

                    MapObject.SetCell(cell);
                }

                if (cell.Darkness != e.Darkness)
                {
                    cell.Darkness = e.Darkness;
                    cell.Updated = true;
                }

                cell.Visible = true;
            }
        }

        private void Handler_MapClear(object sender, MessageHandler.MapLocationEventArgs e)
        {
            var worldX = e.X + _mapScrollX;
            var worldY = e.Y + _mapScrollY;

            //_Logger.Warning($"Map: Cell {worldX}/{worldY} clear");

            lock (_mapDataLock)
            {
                var cell = MapObject.GetCell(worldX, worldY);

                if (cell != null)
                {
                    //instead of removing the cell, set its visibility
                    //to false indicating it is out of the viewport and
                    //part of fog of war. When we recieve a new face or
                    //animation for this cell the visibility will be set
                    //to true as it will be back in the viewport.
                    cell.Visible = false;
                    cell.Updated = true;
                }
            }
        }

        private void Handler_MapClearLayer(object sender, MessageHandler.MapLocationLayerEventArgs e)
        {
            var worldX = e.X + _mapScrollX;
            var worldY = e.Y + _mapScrollY;

            //_Logger.Warning($"Map: Cell {worldX}/{worldY} clear layer {e.Layer}");

            lock (_mapDataLock)
            {
                var cell = MapObject.GetCell(worldX, worldY);

                if (cell != null)
                {
                    cell.GetLayer(e.Layer).ClearLayer();
                    cell.Updated = true;
                }
            }
        }

        private void Handler_MapScroll(object sender, MessageHandler.MapLocationEventArgs e)
        {
            var oldScrollX = _mapScrollX;
            var oldScrollY = _mapScrollY;

            _mapScrollX += e.X;
            _mapScrollY += e.Y;

            MapObject.PlayerX += e.X;
            MapObject.PlayerY += e.Y;

            workingUpdateArgs.MapScrollX = e.X;
            workingUpdateArgs.MapScrollY = e.Y;

            //any cells that were visible prior to this mapscroll, but
            //became invisible after the map scroll should be marked
            //so that we can update them if we get new map data for the
            //co-ordinates. (invisible data is used for fog of war data)
            //Basically the server doesn't expect these cells to exist
            //so we need to clear them
            lock (_mapDataLock)
            {
                //TODO: Remove foreach, narrow search window based on
                //the mapscroll amount
                foreach (var cell in MapObject.Cells)
                {
                    //Check if visible cell has gone out of viewport.
                    //Note this doesn't catch cells that were already
                    //not visible (F.O.W) then moved out of the viewport.
                    if (cell.Visible && !IsMapCellInViewport(cell))
                    {
                        //mark cell as part of fog of war and notify any listeners.
                        //Note that the mapscroll is sent before any cells are
                        //added/cleared/updated, so we have to manually trigger
                        //cell updated here (as these cells are now out of the
                        //viewport and will never be updated)
                        cell.Visible = false;
                        OnMapCellUpdated(cell);
                        workingUpdateArgs.CellLocations.Add(new MapCellLocation(cell.WorldX, cell.WorldY));

                        //Indicate that the mapscroll has caused cells
                        //to change
                        workingUpdateArgs.OutsideViewportChanged = true;
                    }
                }
            }
        }


        private void Handler_Smooth(object sender, MessageHandler.SmoothEventArgs e)
        {
            _smoothFaces.Add(e);
        }

        private void Handler_Tick(object sender, MessageHandler.TickEventArgs e)
        {
            lock (_mapDataLock)
            {
                var args = new MapUpdatedEventArgs();
                bool updated = false;

                //set base properties, other MapUpdatedEventArgs
                //have been updated
                args.Modification = DataModificationTypes.Updated;
                args.Data = MapObject;
                args.UpdatedProperties = null;
                args.TickChanged = true;

                //Update synchronized animations
                var updatedAnimations = new HashSet<UInt16>();

                var updatedSyncedAnimations = MapObject.UpdateSynchronizedAnimations();

                /* On a tick, only update cells in the viewport */
                for (int y = MapObject.ViewportY, dy = 0; dy <= MapObject.Height; y++, dy++)
                {
                    for (int x = MapObject.ViewportX, dx = 0; dx <= MapObject.Width; x++, dx++)
                    {
                        var cell = MapObject.GetCell(x, y);
                        if (cell == null)
                            continue;

                        foreach (var layer in cell.Layers)
                        {
                            if (layer == null)
                                continue;

                            if (!layer.IsAnimation)
                                continue;

                            switch (layer.AnimationType)
                            {
                                case Map.AnimationTypes.Normal:
                                case Map.AnimationTypes.Randomize:
                                    cell.Updated = layer.AnimationState.UpdateAnimation();
                                    break;

                                case Map.AnimationTypes.Synchronize:
                                    if (updatedSyncedAnimations.Contains(layer.Animation))
                                        cell.Updated = true;
                                    break;
                            }

                            if (cell.Updated)
                            {
                                OnMapCellUpdated(cell);
                                args.CellLocations.Add(new MapCellLocation(x, y));
                                args.InsideViewportChanged = true;
                                cell.Updated = false;
                                updated = true;
                            }
                        }
                    }
                }

                if (updated)
                {
                    OnDataChanged(args);
                    OnMapUpdated(args);
                }
            }
        }

        /// <summary>
        /// Checks to see if the MapCell is visible in the Viewport
        /// </summary>
        /// <returns>true if visible</returns>
        private bool IsMapCellInViewport(MapCell mapCell)
        {
            var viewportX = mapCell.WorldX - _mapScrollX;
            var viewportY = mapCell.WorldY - _mapScrollY;

            if (viewportX < 0) return false;
            if (viewportY < 0) return false;
            if (viewportX >= CurrentMapWidth) return false;
            if (viewportY >= CurrentMapHeight) return false;

            return true;
        }

        private bool IsMapCellInViewport(MapCell mapCell, int scrollX, int scrollY)
        {
            var viewportX = mapCell.WorldX - scrollX;
            var viewportY = mapCell.WorldY - scrollY;

            if (viewportX < 0) return false;
            if (viewportY < 0) return false;
            if (viewportX >= CurrentMapWidth) return false;
            if (viewportY >= CurrentMapHeight) return false;

            return true;
        }

        /// <summary>
        /// Checks to see if the MapCell is visible on the map
        /// </summary>
        /// <returns>true if visible</returns>
        private bool IsMapCellVisible(MapCell mapCell)
        {
            var viewportX = mapCell.WorldX - _mapScrollX;
            var viewportY = mapCell.WorldY - _mapScrollY;

            if (viewportX < 0) return false;
            if (viewportY < 0) return false;
            if (viewportX >= CurrentMapWidth + NewServer.MaxHeadOffset) return false;
            if (viewportY >= CurrentMapHeight + NewServer.MaxHeadOffset) return false;

            return true;
        }

        private void OnBeforeMapClear()
        {
            BeforeMapClear?.Invoke(this, new DataUpdatedEventArgs<MapObject>()
            {
                Modification = DataModificationTypes.Updated,
                Data = MapObject,
                UpdatedProperties = null
            });
        }

        private void OnMapCellUpdated(MapCell cell)
        {
            MapCellUpdated?.Invoke(this,
                new MapCellUpdatedEventArgs(cell));
        }

        private void OnMapUpdated(MapUpdatedEventArgs args)
        {
            MapUpdated?.Invoke(this, args);
        }
    }

    public class MapCellUpdatedEventArgs : EventArgs
    {
        public MapCellUpdatedEventArgs(MapCell mapCell)
        {
            MapCell = mapCell;
        }

        public MapCell MapCell { get; }
        public int WorldX => MapCell.WorldX;
        public int WorldY => MapCell.WorldY;
        public bool Visible => MapCell.Visible;
    }

    public class MapCellLocation
    {
        public MapCellLocation(int worldX, int worldY)
        {
            WorldX = worldX;
            WorldY = worldY;
        }

        public int WorldX { get; }
        public int WorldY { get; }
    }

    public class MapUpdatedEventArgs : DataUpdatedEventArgs<MapObject>
    {
        /// <summary>
        /// Locations of all the cells that changed on the map during this update
        /// </summary>
        public List<MapCellLocation> CellLocations { get; set; } = new List<MapCellLocation>();

        /// <summary>
        /// True if the map scrolled during this update
        /// </summary>
        public bool MapScrolled => (MapScrollX != 0) || (MapScrollY != 0);

        /// <summary>
        /// Amount the map scrolled horizonally during this update
        /// </summary>
        public int MapScrollX { get; set; } = 0;

        /// <summary>
        /// Amount the map scrolled vertically during this update
        /// </summary>
        public int MapScrollY { get; set; } = 0;

        /// <summary>
        /// True if the map size changed during this update
        /// </summary>
        public bool MapSizeChanged { get; set; } = false;

        /// <summary>
        /// True if cells inside the viewport changed during this update
        /// </summary>
        public bool InsideViewportChanged { get; set; } = false;

        /// <summary>
        /// True if cells inside the viewport changed during this update
        /// </summary>
        public bool OutsideViewportChanged { get; set; } = false;

        /// <summary>
        /// True if cells changed during a tick
        /// </summary>
        public bool TickChanged { get; set; } = false;

        /// <summary>
        /// Gets the min and max of the cell locations
        /// </summary>
        /// <returns>true if bounds set, false if not</returns>
        public bool GetCellLocationBoundingBox(out int MinWorldX, out int MinWorldY,
            out int MaxWorldX, out int MaxWorldY)
        {
            MinWorldX = int.MaxValue;
            MinWorldY = int.MaxValue;
            MaxWorldX = int.MinValue;
            MaxWorldY = int.MinValue;

            if (CellLocations.Count == 0)
                return false;

            foreach (var cellLocation in CellLocations)
            {
                if (cellLocation.WorldX < MinWorldX)
                    MinWorldX = cellLocation.WorldX;
                if (cellLocation.WorldY < MinWorldY)
                    MinWorldY = cellLocation.WorldY;

                if (cellLocation.WorldX > MaxWorldX)
                    MaxWorldX = cellLocation.WorldX;
                if (cellLocation.WorldY > MaxWorldY)
                    MaxWorldY = cellLocation.WorldY;
            }

            return true;
        }
    }
}

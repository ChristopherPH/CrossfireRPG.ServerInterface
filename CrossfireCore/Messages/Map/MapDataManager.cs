using Common;
using CrossfireCore.ManagedObjects;
using CrossfireCore.ServerConfig;
using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;

namespace CrossfireCore.Managers
{
    public class MapDataManager : DataObjectManager<MapObject>
    {
        public MapDataManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            //Individual events
            Handler.Setup += Handler_Setup;
            Handler.Animation += Handler_Animation;
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
        }

        public static Logger Logger { get; } = new Logger(nameof(MapDataManager));
        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => false;
        public override DataModificationTypes SupportedModificationTypes =>
            DataModificationTypes.MultiCommandStart | DataModificationTypes.MultiCommandEnd |
            DataModificationTypes.Added |   //Raised after a map has been populated for the first time
            DataModificationTypes.Updated | //Raised when a map has been updated
            DataModificationTypes.Cleared;  //Raised when a map has been cleared

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
        Dictionary<UInt16, UInt16[]> Animations = new Dictionary<UInt16, UInt16[]>();
        Dictionary<UInt16, SynchronizedAnimation> _synchronizedAnimations =
            new Dictionary<ushort, SynchronizedAnimation>();
        int _mapScrollX = 0;
        int _mapScrollY = 0;
        int CurrentMapWidth = Config.MAP_CLIENT_X_DEFAULT;
        int CurrentMapHeight = Config.MAP_CLIENT_Y_DEFAULT;
        List<MessageHandler.SmoothEventArgs> _smoothFaces = new List<MessageHandler.SmoothEventArgs>();

        //Private variables for creating map updated args
        List<MapCellLocation> _updatedLocations = new List<MapCellLocation>();
        bool _mapScrollUpdatedMap = false;

        protected override void ClearData(bool disconnected)
        {
            OnBeforeMapClear();

            //Items to clear on new player
            lock (_mapDataLock)
                MapObject.ClearMap();

            _synchronizedAnimations.Clear();
            _mapScrollX = _mapScrollY = 0;

            //Items to clear on server disconnect
            Animations.Clear();
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

                _synchronizedAnimations.Clear();
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

        private void Handler_Animation(object sender, MessageHandler.AnimationEventArgs e)
        {
            Animations[e.AnimationNumber] = e.AnimationFaces;
        }

        private void Handler_MapBegin(object sender, System.EventArgs e)
        {
            _updatedLocations.Clear();
            _mapScrollUpdatedMap = false;

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
            if ((_updatedLocations.Count > 0) || _mapScrollUpdatedMap)
            {
                var args = new MapUpdatedEventArgs()
                {
                    Modification = DataModificationTypes.Updated,
                    Data = MapObject,
                    UpdatedProperties = null,
                    MapScrolled = _mapScrollUpdatedMap,
                    CellLocations = new List<MapCellLocation>(_updatedLocations)
                };

                OnDataChanged(args);
                OnMapUpdated(args);
            }

            _updatedLocations.Clear();
            _mapScrollUpdatedMap = false;
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
                    _updatedLocations.Add(new MapCellLocation(worldX, worldY));
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

            _synchronizedAnimations.Clear();
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
                    IsAnimation = true,
                    Face = e.Animation, //should save animation ID seperate, we want this to be the actual face to render
                    CurrentFrame = 0,
                    AnimationSpeed = e.AnimationSpeed,
                    animationType = (AnimationTypes)e.AnimationType,
                    CurrentTick = 0,
                    SmoothLevel = e.Smooth,
                };

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

            //any cells that were visible prior to this mapscroll, but
            //became invisible after the map scroll should be marked
            //so that we can update them if we get new map data for the
            //co-ordinates. (invisible data is used for fog of war data)
            //Basically the server doesn't expect these cells to exist
            //so we need to clear them
            lock (_mapDataLock)
            {
                foreach (var cell in MapObject.Cells)
                {
                    //check if cell has gone out of viewport
                    if (cell.Visible && !IsMapCellInViewport(cell))
                    {
                        //mark cell as part of fog of war and notify any listeners.
                        //Note that the mapscroll is sent before any cells are
                        //added/cleared/updated, so we have to manually trigger
                        //cell updated here (as these cells are now out of the
                        //viewport and will never be updated)
                        cell.Visible = false;
                        OnMapCellUpdated(cell);
                        _updatedLocations.Add(new MapCellLocation(cell.WorldX, cell.WorldY));
                        _mapScrollUpdatedMap = true;
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
                foreach (var cell in MapObject.Cells)
                {
                    //TODO: Determine if we update animations on 
                    //      FOW or out of map cells
                    if (!cell.Visible)
                        continue;

                    foreach (var layer in cell.Layers)
                    {
                        if (layer == null)
                            continue;

                        if (!layer.IsAnimation)
                            continue;

                        //TODO: update animation

                        cell.Updated = true;
                    }
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
        public List<MapCellLocation> CellLocations { get; set; } = new List<MapCellLocation>();

        public bool MapScrolled { get; set; }
    }
}

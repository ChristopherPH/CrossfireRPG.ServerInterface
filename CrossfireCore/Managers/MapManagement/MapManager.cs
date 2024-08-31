using CrossfireRPG.ServerInterface.Definitions;
using CrossfireRPG.ServerInterface.Managers.AnimationManagement;
using CrossfireRPG.ServerInterface.Managers.MapSizeManagement;
using CrossfireRPG.ServerInterface.Network;
using CrossfireRPG.ServerInterface.Protocol;
using CrossfireRPG.Utility.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossfireRPG.ServerInterface.Managers.MapManagement
{
    public class MapManager : DataObjectManager<MapObject>
    {
        public MapManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler,
            MapSizeManager mapsizeManager, AnimationDataManager animationManager)
            : base(Connection, Builder, Handler)
        {
            //Individual events
            Handler.NewMap += Handler_NewMap;
            Handler.Player += Handler_Player;
            Handler.Tick += Handler_Tick;

            //All part of map2 events
            Handler.MapBegin += Handler_MapBegin;
            Handler.MapEnd += Handler_MapEnd;
            Handler.MapBeginLocation += Handler_MapBeginLocation;
            Handler.MapEndLocation += Handler_MapEndLocation;
            Handler.MapFace += Handler_MapFace;
            Handler.MapAnimation += Handler_MapAnimation;
            Handler.MapDarkness += Handler_MapDarkness;
            Handler.MapLabel += Handler_MapLabel;
            Handler.MapClear += Handler_MapClear;
            Handler.MapClearLayer += Handler_MapClearLayer;
            Handler.MapScroll += Handler_MapScroll;

            //Other managers
            MapsizeManager = mapsizeManager;
            mapsizeManager.MapSizeChanged += MapsizeManager_MapSizeChanged;

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

        public MapSizeManager MapsizeManager { get; }
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
        /// Raised when a map cell label has been updated
        /// </summary>
        public event EventHandler<MapCellUpdatedEventArgs> MapCellLabelUpdated;

        /// <summary>
        /// Raised when the map has been updated : Occurs when ModificationTypes.Updated
        /// is raised, but this event contains additional data
        /// </summary>
        public event EventHandler<MapUpdatedEventArgs> MapUpdated;

        //Private variables
        readonly object _mapDataLock = new object();
        bool _populatingNewMap = false;
        int _mapScrollX = 0;
        int _mapScrollY = 0;

        //Private variables for creating map updated args
        MapUpdatedEventArgs workingUpdateArgs = new MapUpdatedEventArgs();
        bool workingIsEmpty;
        int workingMinX;
        int workingMaxX;
        int workingMinY;
        int workingMaxY;

        //Private variables for handling cell updates
        bool _workingCellUpdated;
        bool _workingCellLayerUpdated;
        List<MapLabel> _savedCellLabels = new List<MapLabel>();

        protected override void ClearData(bool disconnected)
        {
            OnBeforeMapClear();

            //Items to clear on new player
            lock (_mapDataLock)
                MapObject.ClearMap();

            _mapScrollX = _mapScrollY = 0;

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
                    MapObject.SetViewportSize(MapsizeManager.CurrentMapWidth, MapsizeManager.CurrentMapHeight);
                }

                _mapScrollX = _mapScrollY = 0;

                OnDataChanged(DataModificationTypes.Cleared, MapObject);
            }
        }


        private void MapsizeManager_MapSizeChanged(object sender, MapSizeEventArgs e)
        {
            //As it turns out, changing the viewport (mapsize) triggers
            //the newmap command before returning the setup command,
            //so we need to adjust the viewport after the map has been
            //created
            lock (_mapDataLock)
                MapObject.SetViewportSize(e.Width, e.Height);
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

            //Mark cell as not yet updated
            _workingCellUpdated = false;
            _workingCellLayerUpdated = false;
            _savedCellLabels.Clear();

            lock (_mapDataLock)
            {
                var cell = MapObject.GetCell(worldX, worldY);
                if (cell != null)
                {
                    //The server does not send any commands to clear labels,
                    //so the only place to clear the labels is when the map2
                    //first gives cell co-ordinates (which is this event).
                    //Save any existing cell labels, these will be compared
                    //at the end of the map2 command (EndLocation) to
                    //determine if the cell has changed.
                    _savedCellLabels.AddRange(cell.Labels);
                    cell.Labels.Clear();
                }
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
                if (cell != null)
                {
                    //check for changed map labels
                    if (!cell.Labels.SequenceEqual(_savedCellLabels))
                    {
                        OnMapCellLabelUpdated(cell);

                        workingUpdateArgs.CellLabelLocations.Add(
                            new MapUpdatedEventArgs.MapCellLocation(worldX, worldY));
                    }

                    //Notify cell updated
                    if (_workingCellUpdated)
                    {
                        OnMapCellUpdated(cell);

                        workingUpdateArgs.CellLocations.Add(
                            new MapUpdatedEventArgs.MapCellLocation(worldX, worldY));
                        workingUpdateArgs.InsideViewportChanged = true;

                        _workingCellUpdated = false;
                    }
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
                MapObject.SetViewportSize(MapsizeManager.CurrentMapWidth, MapsizeManager.CurrentMapHeight);
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
                (e.X >= MapsizeManager.CurrentMapWidth) ||
                (e.Y >= MapsizeManager.CurrentMapHeight);

            lock (_mapDataLock)
            {
                //Get pre-existing cell at x/y
                var cell = MapObject.GetCell(worldX, worldY);
                if (cell != null)
                {
                    /* If the pre-existing cell was part of FOW data, it means
                     * that it had previously gone out of view.
                     *
                     * If the pre-existing cell was out of bounds, it means
                     * that the server does not know about this cell.
                     * (The server doesn't remember when it sends OOB data so
                     * it doesn't ever clear it.)
                     *
                     * Since we've now gotten information for the cell location,
                     * we need to clear the cell data like the server expects,
                     * so we can start the tile fresh. */
                    if (cell.FogOfWar || cell.OutOfBounds || OutOfBounds)
                    {
                        /* Only clear cell if the cell has not yet been
                         * cleared on this map location update */
                        if (!_workingCellLayerUpdated)
                        {
                            cell.ClearCell();

                            /* Always update on FOW change,
                             * TODO: Check conditions to update on OOB */
                            _workingCellUpdated = true;
                        }
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
                    LayerIndex = e.Layer,
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
                    _workingCellUpdated = true;
                }

                //If the face is out of the visible map area, mark it as
                //out of bounds, and note the server doesn't send a clear
                //command for this data.
                cell.OutOfBounds = OutOfBounds;
                cell.FogOfWar = false;

                //Mark this cell location as updated so its not cleared again
                _workingCellLayerUpdated = true;
            }
        }

        private void Handler_MapAnimation(object sender, MessageHandler.MapAnimationEventArgs e)
        {
            var worldX = e.X + _mapScrollX;
            var worldY = e.Y + _mapScrollY;

            var OutOfBounds = (e.X < 0) || (e.Y < 0) ||
                (e.X >= MapsizeManager.CurrentMapWidth) ||
                (e.Y >= MapsizeManager.CurrentMapHeight);

            lock (_mapDataLock)
            {
                //Get pre-existing cell at x/y
                var cell = MapObject.GetCell(worldX, worldY);
                if (cell != null)
                {
                    /* If the pre-existing cell was part of FOW data, it means
                     * that it had previously gone out of view.
                     *
                     * If the pre-existing cell was out of bounds, it means
                     * that the server does not know about this cell.
                     * (The server doesn't remember when it sends OOB data so
                     * it doesn't ever clear it.)
                     *
                     * Since we've now gotten information for the cell location,
                     * we need to clear the cell data like the server expects,
                     * so we can start the tile fresh. */
                    if (cell.FogOfWar || cell.OutOfBounds || OutOfBounds)
                    {
                        /* Only clear cell if the cell has not yet been
                         * cleared on this map location update */
                        if (!_workingCellLayerUpdated)
                        {
                            cell.ClearCell();

                            /* Always update on FOW change,
                             * TODO: Check conditions to update on OOB */
                            _workingCellUpdated = true;
                        }
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
                    LayerIndex = e.Layer,
                    SmoothLevel = e.Smooth,
                    Animation = e.Animation,
                    AnimationFlags = e.AnimationFlags,
                    AnimationSpeed = e.AnimationSpeed,
                };

                var frameCount = AnimationManager?.GetAnimationFrameCount(layer.Animation) ?? 0;

                if (layer.AnimationFlags.HasFlag(Map.AnimationFlags.Synchronize))
                {
                    MapObject.AddSynchronizedAnimation(layer.Animation, layer.AnimationFlags,
                        layer.AnimationSpeed, frameCount);
                }
                else
                {
                    layer.AnimationState.SetAnimation(layer.AnimationFlags,
                        layer.AnimationSpeed, frameCount);
                }

                if (cell.Layers[e.Layer] != layer)
                {
                    //_Logger.Warning($"Map: Cell {worldX}/{worldY} layer {e.Layer} updated from face {cell.Layers[e.Layer].Face} to {layer.Face}");
                    cell.Layers[e.Layer] = layer;
                    _workingCellUpdated = true;
                }

                //If the face is out of the visible map area, mark it as
                //out of bounds, and note the server doesn't send a clear
                //command for this data.
                cell.OutOfBounds = OutOfBounds;
                cell.FogOfWar = false;

                //Mark this cell location as updated so its not cleared again
                _workingCellLayerUpdated = true;
            }
        }

        private void Handler_MapDarkness(object sender, MessageHandler.MapDarknessEventArgs e)
        {
            var worldX = e.X + _mapScrollX;
            var worldY = e.Y + _mapScrollY;

            var OutOfBounds = (e.X < 0) || (e.Y < 0) ||
                (e.X >= MapsizeManager.CurrentMapWidth) ||
                (e.Y >= MapsizeManager.CurrentMapHeight);

            lock (_mapDataLock)
            {
                //Get pre-existing cell at x/y
                var cell = MapObject.GetCell(worldX, worldY);
                if (cell != null)
                {
                    /* If the pre-existing cell was part of FOW data, it means
                     * that it had previously gone out of view.
                     *
                     * If the pre-existing cell was out of bounds, it means
                     * that the server does not know about this cell.
                     * (The server doesn't remember when it sends OOB data so
                     * it doesn't ever clear it.)
                     *
                     * Since we've now gotten information for the cell location,
                     * we need to clear the cell data like the server expects,
                     * so we can start the tile fresh. */
                    if (cell.FogOfWar || cell.OutOfBounds || OutOfBounds)
                    {
                        /* Only clear cell if the cell has not yet been
                         * cleared on this map location update */
                        if (!_workingCellLayerUpdated)
                        {
                            cell.ClearCell();

                            /* Always update on FOW change,
                             * TODO: Check conditions to update on OOB */
                            _workingCellUpdated = true;
                        }
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
                    _workingCellUpdated = true;
                }

                //If the face is out of the visible map area, mark it as
                //out of bounds, and note the server doesn't send a clear
                //command for this data.
                cell.OutOfBounds = OutOfBounds;
                cell.FogOfWar = false;

                //Mark this cell location as updated so its not cleared again
                _workingCellLayerUpdated = true;
            }
        }

        private void Handler_MapLabel(object sender, MessageHandler.MapLabelEventArgs e)
        {
            var worldX = e.X + _mapScrollX;
            var worldY = e.Y + _mapScrollY;

            var OutOfBounds = (e.X < 0) || (e.Y < 0) ||
                (e.X >= MapsizeManager.CurrentMapWidth) ||
                (e.Y >= MapsizeManager.CurrentMapHeight);

            lock (_mapDataLock)
            {
                lock (_mapDataLock)
                {
                    //Get pre-existing cell at x/y
                    var cell = MapObject.GetCell(worldX, worldY);
                    if (cell != null)
                    {
                        /* If the pre-existing cell was part of FOW data, it means
                         * that it had previously gone out of view.
                         *
                         * If the pre-existing cell was out of bounds, it means
                         * that the server does not know about this cell.
                         * (The server doesn't remember when it sends OOB data so
                         * it doesn't ever clear it.)
                         *
                         * Since we've now gotten information for the cell location,
                         * we need to clear the cell data like the server expects,
                         * so we can start the tile fresh. */
                        if (cell.FogOfWar || cell.OutOfBounds || OutOfBounds)
                        {
                            if (!_workingCellLayerUpdated)
                            {
                                cell.ClearCell();

                                /* Always update on FOW change,
                                 * TODO: Check conditions to update on OOB */
                                _workingCellUpdated = true;
                            }
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

                    //Create label
                    var mapLabel = new MapLabel()
                    {
                        LabelType = e.LabelType,
                        Label = e.Label,
                    };

                    //Append label to cell labels if it doesn't already exist
                    if (!cell.Labels.Contains(mapLabel))
                        cell.Labels.Add(mapLabel);

                    //If the face is out of the visible map area, mark it as
                    //out of bounds, and note the server doesn't send a clear
                    //command for this data.
                    cell.OutOfBounds = OutOfBounds;
                    cell.FogOfWar = false;

                    //Mark this cell location as updated so its not cleared again
                    _workingCellLayerUpdated = true;
                }
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
                    //instead of removing the cell, set the Fog of War
                    //flag indicating it is out of the viewport and
                    //part of fog of war. When we recieve a new face or
                    //animation for this cell the visibility will be set
                    //to true as it will be back in the viewport.
                    if (!cell.FogOfWar)
                    {
                        cell.FogOfWar = true;
                        _workingCellUpdated = true;
                    }
                }
            }
        }

        private void Handler_MapClearLayer(object sender, MessageHandler.MapLocationLayerEventArgs e)
        {
            var worldX = e.X + _mapScrollX;
            var worldY = e.Y + _mapScrollY;

            var OutOfBounds = (e.X < 0) || (e.Y < 0) ||
                (e.X >= MapsizeManager.CurrentMapWidth) ||
                (e.Y >= MapsizeManager.CurrentMapHeight);

            //_Logger.Warning($"Map: Cell {worldX}/{worldY} clear layer {e.Layer}");

            lock (_mapDataLock)
            {
                var cell = MapObject.GetCell(worldX, worldY);

                if (cell != null)
                {
                    /* If the pre-existing cell was part of FOW data, it means
                     * that it had previously gone out of view.
                     *
                     * If the pre-existing cell was out of bounds, it means
                     * that the server does not know about this cell.
                     * (The server doesn't remember when it sends OOB data so
                     * it doesn't ever clear it.)
                     *
                     * Since we've now gotten information for the cell location,
                     * we need to clear the cell data like the server expects,
                     * so we can start the tile fresh. */
                    if (cell.FogOfWar || cell.OutOfBounds || OutOfBounds)
                    {
                        /* Only clear cell if the cell has not yet been
                         * cleared on this map location update */
                        if (!_workingCellLayerUpdated)
                        {
                            cell.ClearCell();

                            /* Always update on FOW change,
                             * TODO: Check conditions to update on OOB */
                            _workingCellUpdated = true;
                        }
                    }

                    //Create a new, empty, cleared layer
                    var layer = new MapLayer()
                    {
                        LayerIndex = e.Layer,
                    };

                    //Check for layer changes
                    if (cell.Layers[e.Layer] != layer)
                    {
                        cell.Layers[e.Layer] = layer;
                        _workingCellUpdated = true;
                    }

                    //If the face is out of the visible map area, mark it as
                    //out of bounds, and note the server doesn't send a clear
                    //command for this data.
                    cell.OutOfBounds = OutOfBounds;
                    cell.FogOfWar = false;

                    //Mark this cell location as updated so its not cleared again
                    _workingCellLayerUpdated = true;
                }
            }
        }

        private void Handler_MapScroll(object sender, MessageHandler.MapLocationEventArgs e)
        {
            var oldViewportX = MapObject.ViewportX;
            var oldViewportY = MapObject.ViewportY;

            _mapScrollX += e.X;
            _mapScrollY += e.Y;

            MapObject.PlayerX += e.X;
            MapObject.PlayerY += e.Y;

            workingUpdateArgs.MapScrollX = e.X;
            workingUpdateArgs.MapScrollY = e.Y;

            //Check for any visible cells that were in the viewport prior to
            //this mapscroll, but have now been scrolled out of the viewport.
            //Instead of claring these cells, mark them as Fog of War data.
            //Basically the server doesn't expect these cells to exist
            //so we need to clear them
            lock (_mapDataLock)
            {
                int srcX = oldViewportX;

                for (int x = 0; x < MapObject.ViewportWidth + NewServer.MaxHeadOffset; x++, srcX++)
                {
                    int srcY = oldViewportY;

                    for (int y = 0; y < MapObject.ViewportHeight + NewServer.MaxHeadOffset; y++, srcY++)
                    {
                        var cell = MapObject.GetCell(srcX, srcY);
                        if (cell == null)
                            continue;

                        //Check if visible cell has gone out of viewport.
                        //Check if OOB cell has been scrolled
                        //Note this doesn't catch cells that were already
                        //not visible (F.O.W) then moved out of the viewport.
                        if (!cell.FogOfWar && !IsMapCellInViewport(cell))
                        {
                            //mark cell as part of fog of war and notify any listeners.
                            //Note that the mapscroll is sent before any cells are
                            //added/cleared/updated, so we have to manually trigger
                            //cell updated here (as these cells are now out of the
                            //viewport and will never be updated)
                            cell.FogOfWar = true;
                            OnMapCellUpdated(cell);
                            workingUpdateArgs.CellLocations.Add(
                                new MapUpdatedEventArgs.MapCellLocation(cell.WorldX, cell.WorldY));

                            //Indicate that the mapscroll has caused cells
                            //to change
                            workingUpdateArgs.OutsideViewportChanged = true;
                        }
                    }
                }
            }
        }

        private void Handler_Tick(object sender, MessageHandler.TickEventArgs e)
        {
            lock (_mapDataLock)
            {
                var args = new MapUpdatedEventArgs();
                var mapUpdated = false;

                //set base properties, other MapUpdatedEventArgs
                //have been updated
                args.Modification = DataModificationTypes.Updated;
                args.Data = MapObject;
                args.UpdatedProperties = null;
                args.TickChanged = true;

                //Update synchronized animations and save set of
                //animations that have actually changed frames
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

                        var cellUpdated = false;

                        foreach (var layer in cell.Layers)
                        {
                            if (layer == null)
                                continue;

                            if (!layer.IsAnimation)
                                continue;

                            if (layer.AnimationFlags.HasFlag(Map.AnimationFlags.Synchronize))
                            {
                                //Check if this animation exists in saved
                                //set of anumations that actually changed
                                //frames
                                if (updatedSyncedAnimations.Contains(layer.Animation))
                                    cellUpdated = true;
                            }
                            else
                            {
                                cellUpdated = layer.AnimationState.UpdateAnimation();
                            }

                            if (cellUpdated)
                            {
                                OnMapCellUpdated(cell);
                                args.CellLocations.Add(new MapUpdatedEventArgs.MapCellLocation(x, y));
                                args.InsideViewportChanged = true;
                                mapUpdated = true;
                            }
                        }
                    }
                }

                if (mapUpdated)
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
            if (viewportX >= MapsizeManager.CurrentMapWidth) return false;
            if (viewportY >= MapsizeManager.CurrentMapHeight) return false;

            return true;
        }

        private bool IsMapCellInViewport(MapCell mapCell, int scrollX, int scrollY)
        {
            var viewportX = mapCell.WorldX - scrollX;
            var viewportY = mapCell.WorldY - scrollY;

            if (viewportX < 0) return false;
            if (viewportY < 0) return false;
            if (viewportX >= MapsizeManager.CurrentMapWidth) return false;
            if (viewportY >= MapsizeManager.CurrentMapHeight) return false;

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
            if (viewportX >= MapsizeManager.CurrentMapWidth + NewServer.MaxHeadOffset) return false;
            if (viewportY >= MapsizeManager.CurrentMapHeight + NewServer.MaxHeadOffset) return false;

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

        private void OnMapCellLabelUpdated(MapCell cell)
        {
            MapCellLabelUpdated?.Invoke(this,
                new MapCellUpdatedEventArgs(cell));
        }

        private void OnMapUpdated(MapUpdatedEventArgs args)
        {
            MapUpdated?.Invoke(this, args);
        }
    }
}

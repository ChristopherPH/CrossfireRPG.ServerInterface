#define MAPOBJECT_SERIALIZATION
using CrossfireRPG.ServerInterface.Definitions;
using CrossfireRPG.Utility.ExpandingArray;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace CrossfireRPG.ServerInterface.Managers.MapManagement
{
    public class MapObject : DataObject
    {
        //Set initial map size to twice that of the maximum viewport (in each quadrant),
        //effectively 4 viewports wide by 4 viewports high
        private readonly Expanding2DArray<MapCell> _cells =
            new Expanding2DArray<MapCell>(Config.MAP_CLIENT_X * 2, Config.MAP_CLIENT_Y * 2);

        public MapObject()
        {
            ClearMap();
        }

        //TODO: may want to consider having the returned minimum width/height
        //      actually be the current mapsize. Since we clear the map, populate
        //      the map and call invalidate, drawing might start drawing before
        //      the map data is all filled in, causing the _cells to only have
        //      a height or width less than the mapsize (as its not full yet)

        [XmlAttribute]
#if MAPOBJECT_SERIALIZATION
        public int MinX { get; set; }
#else
        public int MinX { get; private set; }
#endif

        [XmlAttribute]
#if MAPOBJECT_SERIALIZATION
        public int MinY { get; set; }
#else
        public int MinY { get; private set; }
#endif

        [XmlAttribute]
#if MAPOBJECT_SERIALIZATION
        public int MaxX { get; set; }
#else
        public int MaxX { get; private set; }
#endif

        [XmlAttribute]
#if MAPOBJECT_SERIALIZATION
        public int MaxY { get; set; }
#else
        public int MaxY { get; private set; }
#endif

        public int Width => IsEmpty ? 0 : MaxX - MinX + 1;
        public int Height => IsEmpty ? 0 : MaxY - MinY + 1;

        public int CenterX => MinX + (Width / 2);
        public int CenterY => MinY + (Height / 2);

        public int ViewportX => PlayerX - (ViewportWidth / 2);
        public int ViewportY => PlayerY - (ViewportHeight / 2);


#if MAPOBJECT_SERIALIZATION
        [XmlAttribute]
#endif
        public int ViewportWidth { get; set; }

#if MAPOBJECT_SERIALIZATION
        [XmlAttribute]
#endif
        public int ViewportHeight { get; set; }

#if MAPOBJECT_SERIALIZATION
        [XmlAttribute]
#endif
        public int PlayerX { get; set; }

#if MAPOBJECT_SERIALIZATION
        [XmlAttribute]
#endif
        public int PlayerY { get; set; }


        /// <summary>
        /// Gets all non null cells in the map
        /// </summary>
        [XmlIgnore]
        public IEnumerable<MapCell> Cells => _cells.Where(x => x != null);


        private readonly Dictionary<UInt16, MapAnimationState> _SynchronizedAnimations
            = new Dictionary<UInt16, MapAnimationState>();

        public void AddSynchronizedAnimation(UInt16 Animation, Map.AnimationFlags AnimationFlags,
            byte AnimationSpeed, int FrameCount)
        {
            if (!_SynchronizedAnimations.ContainsKey(Animation))
            {
                _SynchronizedAnimations[Animation] = new MapAnimationState(
                    AnimationFlags, AnimationSpeed, FrameCount);
            }
        }

        public HashSet<UInt16> UpdateSynchronizedAnimations()
        {
            var updatedAnimations = new HashSet<UInt16>();

            foreach (var kv in _SynchronizedAnimations)
                if (kv.Value.UpdateAnimation())
                    updatedAnimations.Add(kv.Key);

            return updatedAnimations;
        }

        public int GetSynchronizedAnimationFrame(UInt16 Animation)
        {
            if (_SynchronizedAnimations.TryGetValue(Animation, out var syncAnimation))
                return syncAnimation.CurrentFrame;

            //Return the first frame of the animation if the animation
            //was not able to be found
            return 0;
        }

#if MAPOBJECT_SERIALIZATION
        /// <summary>
        /// Create a proxy property for cell values.
        /// Use an array instead of a list so the setter is called during deserialization
        /// (In a list, deserialization calls the getter then adds items to it)
        /// </summary>
        public MapCell[] Cells2
        {
            get => Cells.OrderBy(x => x.WorldX).ThenBy(x => x.WorldY).ToArray();
            set
            {
                _cells.Reset();

                if (value != null)
                {
                    foreach (var cell in value)
                    {
                        _cells[cell.WorldX, cell.WorldY] = cell;
                        IsEmpty = false;
                    }
                }
            }
        }
#endif

        [XmlIgnore]
#if MAPOBJECT_SERIALIZATION
        public bool IsEmpty { get; set; } = true;
#else
        public bool IsEmpty => _cells.IsEmpty;
#endif

        public void ClearMap()
        {
            _cells.Reset();
            MinX = 0;
            MinY = 0;
            MaxX = 0;
            MaxY = 0;
            PlayerX = 0;
            PlayerY = 0;
            _SynchronizedAnimations.Clear();
#if MAPOBJECT_SERIALIZATION
            IsEmpty = true;
#endif
        }

        public void SetViewportSize(int ViewportWidth, int ViewportHeight)
        {
            this.ViewportWidth = ViewportWidth;
            this.ViewportHeight = ViewportHeight;
            this.PlayerX = ViewportWidth / 2;
            this.PlayerY = ViewportHeight / 2;
        }

        public MapCell GetCell(int WorldX, int WorldY)
        {
            if (_cells.TryGetElement(WorldX, WorldY, out var mapCell))
                return mapCell;

            return null;
        }

        public MapCell GetOrCreateCell(int WorldX, int WorldY)
        {
            var mapCell = GetCell(WorldX, WorldY);
            if (mapCell != null)
                return mapCell;

            mapCell = new MapCell()
            {
                WorldX = WorldX,
                WorldY = WorldY,
            };

            _cells[mapCell.WorldX, mapCell.WorldY] = mapCell;

#if MAPOBJECT_SERIALIZATION
            IsEmpty = false;
#endif

            if (WorldX < MinX) MinX = WorldX;
            if (WorldY < MinY) MinY = WorldY;
            if (WorldX > MaxX) MaxX = WorldX;
            if (WorldY > MaxY) MaxY = WorldY;

            return mapCell;
        }

        public void SetCell(MapCell MapCell)
        {
            if (MapCell == null)
                return;

            _cells[MapCell.WorldX, MapCell.WorldY] = MapCell;

#if MAPOBJECT_SERIALIZATION
            IsEmpty = false;
#endif

            if (MapCell.WorldX < MinX) MinX = MapCell.WorldX;
            if (MapCell.WorldY < MinY) MinY = MapCell.WorldY;
            if (MapCell.WorldX > MaxX) MaxX = MapCell.WorldX;
            if (MapCell.WorldY > MaxY) MaxY = MapCell.WorldY;
        }

        /// <summary>
        /// Gets a cell using tile co-ordinates relative to the player
        /// </summary>
        /// <param name="RelativeX">Tile offset relative to player - player is at tile co-ordinates 0,0</param>
        /// <param name="RelativeY">Tile offset relative to player - player is at tile co-ordinates 0,0</param>
        /// <returns>Map Cell</returns>
        public MapCell GetRelativeCell(int RelativeX, int RelativeY)
        {
            var worldX = PlayerX + RelativeX;
            var worldY = PlayerY + RelativeY;

            if (_cells.TryGetElement(worldX, worldY, out var mapCell))
                return mapCell;

            return null;
        }

        /// <summary>
        /// Gets cell using tile co-ordinates within the viewport
        /// </summary>
        /// <param name="ViewportX">Tile offset within viewport - top left is tile co-ordinates 0,0</param>
        /// <param name="ViewportY">Tile offset within viewport - top left is tile co-ordinates 0,0</param>
        /// <returns></returns>
        public MapCell GetViewportCell(int ViewportX, int ViewportY)
        {
            var worldX = PlayerX - (ViewportWidth / 2) + ViewportX;
            var worldY = PlayerY - (ViewportHeight / 2) + ViewportY;

            if (_cells.TryGetElement(worldX, worldY, out var mapCell))
                return mapCell;

            return null;
        }

        /// <summary>
        /// Given a map cell, get the co-ordinates of the cell relative to the player
        /// </summary>
        /// <param name="MapCell">Cell to get relative co-ordinates of</param>
        /// <param name="RelativeX">Variable to store relative co-ordinates - player is at tile co-ordinates 0,0</param>
        /// <param name="RelativeY">Variable to store relative co-ordinates - player is at tile co-ordinates 0,0</param>
        /// <returns>true if map cell is valid</returns>
        public bool GetRelativeCoordinates(MapCell MapCell, out int RelativeX, out int RelativeY)
        {
            RelativeX = RelativeY = 0;

            if (MapCell == null)
                return false;

            RelativeX = MapCell.WorldX - PlayerX;
            RelativeY = MapCell.WorldY - PlayerY;

            return true;
        }

        /// <summary>
        /// Given a map cell, get the co-ordinates of the cell in the viewport
        /// </summary>
        /// <param name="MapCell">Cell to get viewport co-ordinates of</param>
        /// <param name="ViewportX">Variable to store viewport co-ordinates - top left is tile co-ordinates 0,0</param>
        /// <param name="ViewportY">Variable to store viewport co-ordinates - top left is tile co-ordinates 0,0</param>
        /// <returns>true if map cell is valid</returns>
        public bool GetViewportCoordinates(MapCell MapCell, out int ViewportX, out int ViewportY)
        {
            ViewportX = ViewportY = 0;

            if (MapCell == null)
                return false;

            ViewportX = MapCell.WorldX + (ViewportWidth / 2) - PlayerX;
            ViewportY = MapCell.WorldY + (ViewportHeight / 2) - PlayerY;

            return true;
        }


        public MapObject SaveMap()
        {
            var map = new MapObject()
            {
                ViewportHeight = this.ViewportHeight,
                ViewportWidth = this.ViewportWidth,
                PlayerX = this.PlayerX,
                PlayerY = this.PlayerY,
            };

            //copy sync animations
            //HACK: save a list to avoid enumeration change errors
            foreach (var kv in _SynchronizedAnimations.ToList())
            {
                _SynchronizedAnimations[kv.Key] =
                    kv.Value.SaveMapAnimationState();
            }

            //copy cells
            foreach (var cell in this.Cells)
            {
                map.SetCell(cell.SaveCell());
            }

            return map;
        }
    }
}

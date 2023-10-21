#define MAPOBJECT_SERIALIZATION
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace CrossfireCore.ManagedObjects
{
    public class MapObject
    {
        //private SortedDictionary<MapCoord, MapCell> _cells = new SortedDictionary<MapCoord, MapCell>(new MapCoordComparer());
        //private Dictionary<MapCoord, MapCell> _cells = new Dictionary<MapCoord, MapCell>();
        private Expanding2DArray<MapCell> _cells = new Expanding2DArray<MapCell>(50, 50);

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

        [XmlIgnore]
        public IEnumerable<MapCell> Cells => _cells.Where(x => x != null);

#if MAPOBJECT_SERIALIZATION
        /// <summary>
        /// Create a proxy property for cell values.
        /// Use an array instead of a list so the setter is called during deserialization
        /// (In a list, deserialization calls the getter then adds items to it)
        /// </summary>
        public MapCell[] Cells2
        {
            get => _cells.Where(x => x != null).OrderBy(x => x.WorldX).ThenBy(x => x.WorldY).ToArray();
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

        /*
        private struct MapCoord
        {
            public int WorldX { get; set; }
            public int WorldY { get; set; }
        }

        private class MapCoordComparer : IComparer<MapCoord>
        {
            public int Compare(MapCoord x, MapCoord y)
            {
                if ((x.WorldX == y.WorldX) && (x.WorldY == y.WorldY))
                    return 0;

                if ((x.WorldX < y.WorldX) || ((x.WorldX == y.WorldX) && (x.WorldY < y.WorldY)))
                    return -1;

                return 1;
            }
        }
        */

        public MapObject SaveMap()
        {
            var map = new MapObject()
            {
                ViewportHeight = this.ViewportHeight,
                ViewportWidth = this.ViewportWidth,
                PlayerX = this.PlayerX,
                PlayerY = this.PlayerY,
            };

            foreach (var cell in this.Cells)
            {
                map.SetCell(cell.SaveCell());
            }

            return map;
        }
    }

    //TODO: Consider moving to a struct, as this
    //      already provides an equals and ==
    public class MapLayer
    {
        [XmlAttribute]
        public int Face { get; set; } = 0;

        [XmlAttribute]
        public int SmoothLevel { get; set; } = 0;

        [XmlAttribute]
        public bool IsAnimation { get; set; } = false;

        [XmlIgnore]
        public byte AnimationSpeed { get; set; } = 0;

        [XmlIgnore]
        public AnimationTypes animationType { get; set; } = AnimationTypes.Normal;

        [XmlIgnore]
        public int CurrentFrame { get; set; } = 0;

        [XmlIgnore]
        public int CurrentTick { get; set; } = 0;

        public void ClearLayer()
        {
            Face = 0;
            SmoothLevel = 0;
            IsAnimation = false;
            AnimationSpeed = 0;
            animationType = AnimationTypes.Normal;

            CurrentFrame = 0;
            CurrentTick = 0;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return Equals(obj as MapLayer);
        }

        public bool Equals(MapLayer other)
        {
            return
                Face == other.Face &&
                SmoothLevel == other.SmoothLevel &&
                IsAnimation == other.IsAnimation &&
                AnimationSpeed == other.AnimationSpeed &&
                animationType == other.animationType;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return Tuple.Create(Face, SmoothLevel, IsAnimation, AnimationSpeed, animationType).GetHashCode();
        }

        public static bool operator ==(MapLayer x, MapLayer y)
        {
            if (x is null && y is null) return true;
            if (x is null || y is null) return false;

            return x.Equals(y);
        }

        public static bool operator !=(MapLayer x, MapLayer y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            if (IsAnimation)
                return $"Anim: {Face}";
            else
                return $"Face: {Face}";
        }

        public MapLayer SaveLayer()
        {
            return new MapLayer()
            {
                Face = this.Face,
                SmoothLevel = this.SmoothLevel,
                IsAnimation = this.IsAnimation,
                AnimationSpeed = this.AnimationSpeed,
                animationType = this.animationType,
                CurrentFrame = this.CurrentFrame,
                CurrentTick = this.CurrentTick,
            };
        }
    }

    public class MapCell
    {
        public MapCell()
        {
            ClearCell();
        }

        [XmlAttribute]
        public int WorldX { get; set; } = 0;

        [XmlAttribute]
        public int WorldY { get; set; } = 0;

        [XmlIgnore]
        public bool Visible { get; set; } = false;

#if MAPOBJECT_SERIALIZATION
        public MapLayer[] Layers { get; set; } = new MapLayer[ServerConfig.Map.MAP_LAYERS];
#else
        public MapLayer[] Layers { get; private set; } = new MapLayer[ServerConfig.Map.MAP_LAYERS];
#endif

        [XmlIgnore]
        public int Darkness { get; set; } = 0;

        public void ClearCell()
        {
            WorldX = 0;
            WorldY = 0;
            ClearDarkness();
            ClearLayers();
        }

        public void ClearDarkness()
        {
            Darkness = 0;
        }

        public void ClearLayers()
        {
            Layers = new MapLayer[ServerConfig.Map.MAP_LAYERS];

            for (int i = 0; i < Layers.Length; i++)
                Layers[i] = new MapLayer();
        }

        public MapLayer GetLayer(int layer)
        {
            return Layers[layer];
        }

#if UNUSED
        public void BeginUpdate() { }
        public void EndUpdate() { }
#endif

        [XmlIgnore]
        public bool NeedsUpdate;

        public override string ToString()
        {
            return $"Cell:{WorldX}/{WorldY} Vis:{Visible}";
        }

        public MapCell SaveCell()
        {
            var cell = new MapCell()
            {
                WorldX = this.WorldX,
                WorldY = this.WorldY,
                Visible = this.Visible,
                Darkness = this.Darkness,
            };

            for (int i = 0; i < this.Layers.Length; i++)
                cell.Layers[i] = this.Layers[i].SaveLayer();

            return cell;
        }
    }

    public class SynchronizedAnimation
    {
        public UInt16 Animation { get; set; }
        public byte AnimationSpeed { get; set; } //in ticks (1 = every tick)
        public int CurrentTick { get; set; } = 0;
        public int CurrentFrame { get; set; } = 0;
    }

    public enum AnimationTypes
    {
        Normal = 0,
        Randomize = 1,
        Synchronize = 2
    }

#if UNUSED

    /// <summary>
    /// Base class for a map space
    /// </summary>
    public abstract class MapSpace
    {
        /// <summary>
        /// World X position on the current map, relative to initial spawn point
        /// </summary>
        public int WorldX { get; set; }

        /// <summary>
        /// World Y position on the current map, relative to initial spawn point
        /// </summary>
        public int WorldY { get; set; }

        /// <summary>
        /// Flag if the map info is visible - used for FogOfWar
        /// </summary>
        public bool Visible { get; set; } = true;
    }

    /// <summary>
    /// Base class for a map space on a given map layer
    /// </summary>
    //TODO: consider using interfaces for layer, smoothlevel, face
    public abstract class MapLayerSpace : MapSpace
    {
        public int Layer { get; set; }
    }


    public class MapDarknessSpace : MapSpace
    {
        /// <summary>
        /// 0 is completely dark, 255 is full bright
        /// </summary>
        public byte Darkness { get; set; }

        public override string ToString()
        {
            return string.Format("MapDarkness: X/Y:{0}/{1} Visible:{2} Dark:{3}",
                WorldX, WorldY, Visible, Darkness);
        }
    }

    public abstract class MapGraphicsSpace : MapLayerSpace
    {
        /// <summary>
        /// 0 (overlap nothing) to 255 (overlap above everything 
        /// except other objects having also smoothlevel of 255)
        /// </summary>
        public byte SmoothLevel { get; set; }
    }

    public class MapFaceSpace : MapGraphicsSpace
    {
        public int Face { get; set; }

        public override string ToString()
        {
            return string.Format("MapFace: X/Y:{0}/{1} Visible:{2} Layer:{3} Face:{4}",
                WorldX, WorldY, Visible, Layer, Face);
        }
    }

    public class MapAnimationSpace : MapGraphicsSpace
    {
        public UInt16 Animation { get; set; }
        public AnimationTypes AnimationType { get; set; }
        public byte AnimationSpeed { get; set; } //in ticks (1 = every tick)

        public int CurrentTick { get; set; } = 0;
        public int CurrentFrame { get; set; } = 0;

        public override string ToString()
        {
            return string.Format("MapAnimation: X/Y:{0}/{1} Visible:{2} Layer:{3} Animation:{4}",
                WorldX, WorldY, Visible, Layer, Animation);
        }
    }


    [Flags]
    public enum SmoothDirections
    {
        None = 0x00,
        Left = 0x01,
        Top = 0x02,
        Right = 0x04,
        Bottom = 0x08,
    }


    /*
cells = new MapCell[25, 25];

for (int i = 0; i < cells.GetLength(0); i++)
    for (int j = 0; j < cells.GetLength(1); j++)
        cells[i, j] = new MapCell();

int[] lowerBounds = { 2008 };
int[] lengths = { 10 };
var arr = Array.CreateInstance(typeof(int), lowerBounds, lengths);
arr.GetLowerBound(0);
arr.GetLength(0);

    public class maphorizontal
    {
        List<MapLayer> positive = new List<MapLayer>();
        List<MapLayer> negative = new List<MapLayer>();

        public MapLayer get(int position)
        {
            if (position < 0)
                return negative[position * -1 - 1];
            else
                return positive[position];
        }
    }

    public class mapvertical
    {
        List<maphorizontal> positive = new List<maphorizontal>();
        List<maphorizontal> negative = new List<maphorizontal>();

        maphorizontal get(int position)
        {
            if (position < 0)
                return negative[position * -1 - 1];
            else
                return positive[position];
        }

        MapLayer get(int x, int y)
        {
            var yy = get(y);
            return yy.get(y);
        }
    }
*/
#endif
}

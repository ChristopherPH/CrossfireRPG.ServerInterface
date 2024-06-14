#define MAPOBJECT_SERIALIZATION
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CrossfireCore.Managers.MapManagement
{
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

        /// <summary>
        /// Flag indicating cell is part of Fog of War information,
        /// and is not technically visible in the viewport.
        /// The server has sent a message to clear this cell but
        /// instead of clearing, this flag is set.
        /// </summary>
        [XmlIgnore]
        public bool FogOfWar { get; set; } = false;

        /// <summary>
        /// Flag indicating cell was sent to populate multi-tile
        /// spaces that are inside the viewport, but the cell is
        /// not actually inside the viewport. Note that an OOB
        /// cell could also become a Fog of War cell.
        /// </summary>
        [XmlIgnore]
        public bool OutOfBounds { get; set; } = false;

#if MAPOBJECT_SERIALIZATION
        public MapLayer[] Layers { get; set; } = new MapLayer[ServerConfig.Map.MAP_LAYERS];
#else
        public MapLayer[] Layers { get; private set; } = new MapLayer[ServerConfig.Map.MAP_LAYERS];
#endif

        /// <summary>
        /// 0=dark, 255=light
        /// </summary>
        [XmlIgnore]
        public int Darkness { get; set; } = 0;

        public void ClearCell()
        {
            WorldX = 0;
            WorldY = 0;
            FogOfWar = false;
            OutOfBounds = false;
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

        /// <summary>
        /// Flag indicating a map2 command has updated this cell
        /// </summary>
        [XmlIgnore]
        public bool Updated;

        public override string ToString()
        {
            return $"Cell:{WorldX}/{WorldY} FOW:{FogOfWar}";
        }

        public MapCell SaveCell()
        {
            var cell = new MapCell()
            {
                WorldX = this.WorldX,
                WorldY = this.WorldY,
                FogOfWar = this.FogOfWar,
                OutOfBounds = this.OutOfBounds,
                Darkness = this.Darkness,
            };

            for (int i = 0; i < this.Layers.Length; i++)
                cell.Layers[i] = this.Layers[i].SaveLayer();

            return cell;
        }
    }
}

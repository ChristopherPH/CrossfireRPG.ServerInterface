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

        [XmlIgnore]
        public bool Visible { get; set; } = false;

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
}

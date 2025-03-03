/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
#define MAPOBJECT_SERIALIZATION
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CrossfireRPG.ServerInterface.Managers.MapManagement
{
    public class MapCell
    {
        public MapCell()
        {
            WorldX = 0;
            WorldY = 0;

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

        /// <summary>
        /// 0=dark, 255=light
        /// </summary>
        [XmlIgnore]
        public int Darkness { get; set; } = 0;

        /// <summary>
        /// Magic Map Cell Info
        /// </summary>
        [XmlIgnore]
        public MagicMapCell MagicMap { get; set; } = null;

#if MAPOBJECT_SERIALIZATION
        public MapLayer[] Layers { get; set; } = new MapLayer[Definitions.Map.MAP_LAYERS];
#else
        public MapLayer[] Layers { get; private set; } = new MapLayer[Definitions.Map.MAP_LAYERS];
#endif

        [XmlIgnore]
        public List<MapLabel> Labels { get; } = new List<MapLabel>();

        public void ClearCell()
        {
            FogOfWar = false;
            OutOfBounds = false;
            Darkness = 0;
            MagicMap = null;

            Layers = new MapLayer[Definitions.Map.MAP_LAYERS];

            for (int i = 0; i < Layers.Length; i++)
                Layers[i] = new MapLayer() { LayerIndex = i };

            Labels.Clear();
        }

        public MapLayer GetLayer(int layer)
        {
            return Layers[layer];
        }

#if UNUSED
        public void BeginUpdate() { }
        public void EndUpdate() { }
#endif

        public override string ToString()
        {
            return $"Cell:{WorldX}/{WorldY} FOW:{FogOfWar} Dark:{Darkness} OOB:{OutOfBounds}";
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

            cell.Labels.Clear();
            cell.Labels.AddRange(this.Labels);

            if (cell.MagicMap != null)
            {
                cell.MagicMap = new MagicMapCell()
                {
                    MagicMapInfo = this.MagicMap.MagicMapInfo
                };
            }

            return cell;
        }
    }
}

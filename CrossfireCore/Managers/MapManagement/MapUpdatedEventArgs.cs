using System.Collections.Generic;

namespace CrossfireRPG.ServerInterface.Managers.MapManagement
{
    public class MapUpdatedEventArgs : DataUpdatedEventArgs<MapObject>
    {
        /// <summary>
        /// Locations of all the cells that changed on the map during this update
        /// </summary>
        public List<MapCellLocation> CellLocations { get; set; } = new List<MapCellLocation>();

        /// <summary>
        /// Locations of all the cells labels that changed on the map during this update
        /// </summary>
        public List<MapCellLocation> CellLabelLocations { get; set; } = new List<MapCellLocation>();

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

        /// <summary>
        /// Gets the min and max of the cell label locations
        /// </summary>
        /// <returns>true if bounds set, false if not</returns>
        public bool GetCellLabelLocationBoundingBox(out int MinWorldX, out int MinWorldY,
            out int MaxWorldX, out int MaxWorldY)
        {
            MinWorldX = int.MaxValue;
            MinWorldY = int.MaxValue;
            MaxWorldX = int.MinValue;
            MaxWorldY = int.MinValue;

            if (CellLabelLocations.Count == 0)
                return false;

            foreach (var cellLabelLocation in CellLabelLocations)
            {
                if (cellLabelLocation.WorldX < MinWorldX)
                    MinWorldX = cellLabelLocation.WorldX;
                if (cellLabelLocation.WorldY < MinWorldY)
                    MinWorldY = cellLabelLocation.WorldY;

                if (cellLabelLocation.WorldX > MaxWorldX)
                    MaxWorldX = cellLabelLocation.WorldX;
                if (cellLabelLocation.WorldY > MaxWorldY)
                    MaxWorldY = cellLabelLocation.WorldY;
            }

            return true;
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
    }
}

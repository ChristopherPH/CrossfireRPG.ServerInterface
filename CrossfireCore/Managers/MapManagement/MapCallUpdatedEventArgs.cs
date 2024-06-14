using System;

namespace CrossfireCore.Managers.MapManagement
{
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
}

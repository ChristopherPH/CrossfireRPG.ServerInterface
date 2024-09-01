/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System;

namespace CrossfireRPG.ServerInterface.Managers.MapManagement
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
    }
}

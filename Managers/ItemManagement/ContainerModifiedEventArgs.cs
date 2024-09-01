/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System;

namespace CrossfireRPG.ServerInterface.Managers.ItemManagement
{
    public class ContainerModifiedEventArgs : EventArgs
    {
        public enum ModificationTypes
        {
            Opened,
            Closed
        }

        public ModificationTypes Modification { get; set; }
        public Item Item { get; set; }
        public UInt32 ItemTag => Item.Tag;
        public int ItemIndex { get; set; }

        public override string ToString()
        {
            return string.Format("ContainerModified[{0}]: {1}", Modification, Item);
        }
    }
}

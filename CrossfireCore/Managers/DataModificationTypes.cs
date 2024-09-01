/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System;

namespace CrossfireRPG.ServerInterface.Managers
{
    [Flags]
    public enum DataModificationTypes
    {
        None = 0x00,

        Added = 0x01,
        Updated = 0x02,
        Removed = 0x04,
        Cleared = 0x08,

        MultiCommandStart = 0x10,
        MultiCommandEnd = 0x20,
        GroupUpdateStart = 0x40,
        GroupUpdateEnd = 0x80,
    }
}

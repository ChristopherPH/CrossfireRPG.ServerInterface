/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using CrossfireRPG.ServerInterface.Definitions;

namespace CrossfireRPG.ServerInterface.Managers.MapManagement
{
    public struct MapLabel
    {
        public NewClient.Map2Type_Label LabelType { get; set; }

        public string Label { get; set; }
    }
}

/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
namespace CrossfireRPG.ServerInterface.Definitions
{
    /// <summary>
    /// Constants from CrossfireRPG server include file: config.h
    /// </summary>
    public static class Config
    {
        public const int CSPORT = 13327;

        /* Maximum map size */
        public const int MAP_CLIENT_X = 25;
        public const int MAP_CLIENT_Y = 25;

        /* Default map size */
        public const int MAP_CLIENT_X_DEFAULT = 11;
        public const int MAP_CLIENT_Y_DEFAULT = 11;
        public const string MAP_CLIENT_DEFAULT = "11, 11";

        /* Minimum map size */
        public const int MAP_CLIENT_X_MINIMUM = 9;
        public const int MAP_CLIENT_Y_MINIMUM = 9;
    }
}

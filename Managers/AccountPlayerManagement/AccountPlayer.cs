/*
 * Copyright (c) 2025 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System;

namespace CrossfireRPG.ServerInterface.Managers.AccountPlayerManagement
{
    /// <summary>
    /// Hols information about a player in an account.
    /// </summary>
    public class AccountPlayer : DataObject
    {
        /// <summary>
        /// 1 Based index of player
        /// </summary>
        public int PlayerNumber
        {
            get => _PlayerNumber;
            set => SetProperty(ref _PlayerNumber, value);
        }
        int _PlayerNumber;

        /// <summary>
        /// Player Level
        /// </summary>
        public UInt16 Level
        {
            get => _Level;
            set => SetProperty(ref _Level, value);
        }
        UInt16 _Level;

        /// <summary>
        /// Face Number (image number) of the player.
        /// </summary>
        public UInt32 FaceNumber
        {
            get => _FaceNumber;
            set => SetProperty(ref _FaceNumber, value);
        }
        UInt32 _FaceNumber;

        /// <summary>
        /// Player Name
        /// </summary>
        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }
        string _Name;

        /// <summary>
        /// Player Class (currently not populated)
        /// </summary>
        public string Class
        {
            get => _Class;
            set => SetProperty(ref _Class, value);
        }
        string _Class;

        /// <summary>
        /// Player Race
        /// </summary>
        public string Race
        {
            get => _Race;
            set => SetProperty(ref _Race, value);
        }
        string _Race;

        /// <summary>
        /// Face string for the player (eg "evoker.111")
        /// </summary>
        public string Face
        {
            get => _Face;
            set => SetProperty(ref _Face, value);
        }
        string _Face;

        /// <summary>
        /// Party the player is in (if any).
        /// </summary>
        public string Party
        {
            get => _Party;
            set => SetProperty(ref _Party, value);
        }
        string _Party;

        /// <summary>
        /// Last Map the player was on.
        /// </summary>
        public string Map
        {
            get => _Map;
            set => SetProperty(ref _Map, value);
        }
        string _Map;

        public override string ToString() =>
            $"{Name}:{Level} {Class}:{Race} {Map}";
    }
}

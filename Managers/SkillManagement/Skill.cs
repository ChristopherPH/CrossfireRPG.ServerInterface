/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System;

namespace CrossfireRPG.ServerInterface.Managers.SkillManagement
{
    public class Skill : DataObject
    {
        public int SkillID
        {
            get => _SkillID;
            set => SetProperty(ref _SkillID, value);
        }
        private int _SkillID;

        public Byte Level
        {
            get => _Level;
            set => SetProperty(ref _Level, value);
        }
        private Byte _Level;

        public UInt64 Value
        {
            get => _Value;
            set => SetProperty(ref _Value, value);
        }
        private UInt64 _Value;

        public Byte OldLevel
        {
            get => _OldLevel;
            set => SetProperty(ref _OldLevel, value);
        }
        private Byte _OldLevel;

        public UInt64 OldValue
        {
            get => _OldValue;
            set => SetProperty(ref _OldValue, value);
        }
        private UInt64 _OldValue;
    }
}

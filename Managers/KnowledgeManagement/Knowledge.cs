﻿/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System;

namespace CrossfireRPG.ServerInterface.Managers.KnowledgeManagement
{
    public class Knowledge : DataObject
    {
        public UInt32 KnowledgeID { get => _KnowledgeID; set => SetProperty(ref _KnowledgeID, value); }
        private UInt32 _KnowledgeID;

        public string Type { get => _Type; set => SetProperty(ref _Type, value); }
        private string _Type;

        public string Title { get => _Title; set => SetProperty(ref _Title, value); }
        private string _Title;

        public UInt32 Face { get => _Face; set => SetProperty(ref _Face, value); }
        private UInt32 _Face;


        public override string ToString()
        {
            return $"{Title} {KnowledgeID} {Type} {Face}";
        }
    }
}

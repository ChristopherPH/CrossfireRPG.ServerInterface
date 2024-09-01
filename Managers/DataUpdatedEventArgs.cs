/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System;
using System.Collections.Generic;

namespace CrossfireRPG.ServerInterface.Managers
{
    public class DataUpdatedEventArgs<TDataObject> : EventArgs
    {
        public DataModificationTypes Modification { get; set; }
        public TDataObject Data { get; set; } = default;

        /// <summary>
        /// List of properties that were updated, null indicates all/unknown properties, so best to update everything
        /// </summary>
        public ICollection<string> UpdatedProperties { private get; set; } = null;

        public bool AllUpdated => UpdatedProperties == null;

        public bool WasUpdated(string Property)
        {
            return (UpdatedProperties == null) || UpdatedProperties.Contains(Property);
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}]: {2}", Data.GetType().Name, Modification, Data);
        }
    }

    public class DataListUpdatedEventArgs<T> : DataUpdatedEventArgs<T>
    {
        public int Index { get; set; } = -1;
    }
}

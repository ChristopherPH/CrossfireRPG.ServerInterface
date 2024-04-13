﻿using System;
using System.Collections.Generic;

namespace CrossfireCore.Managers
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

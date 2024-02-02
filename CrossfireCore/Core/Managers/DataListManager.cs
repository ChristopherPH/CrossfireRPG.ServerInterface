using CrossfireCore.ServerInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CrossfireCore.Managers
{
    /// <summary>
    /// Manager class that holds, manages and organizes data for multiple objects
    /// Managers are used to combine multiple server messages/events into a single object
    /// with less updates
    /// </summary>
    public abstract class DataListManager<TDataKey, TDataObject, TDataList> :
        DataObjectManager<TDataObject>, IEnumerable<TDataObject>
        where TDataObject : DataObject
        where TDataList : IList<TDataObject>, new()
    {
        public DataListManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Handler.BeginParseBuffer += Handler_BeginParseBuffer;
            Handler.EndParseBuffer += Handler_EndParseBuffer;
        }

        /// <summary>
        /// Setup property to indicate what events the manager will trigger
        /// </summary>
        public override DataModificationTypes SupportedModificationTypes =>
            base.SupportedModificationTypes | DataModificationTypes.Cleared |
                DataModificationTypes.GroupUpdateStart | DataModificationTypes.GroupUpdateEnd;

        private TDataList _DataObjects { get; } = new TDataList();
        private readonly Dictionary<TDataKey, TDataObject> _DataLookup = new Dictionary<TDataKey, TDataObject>();
        private readonly object _DataObjectLock = new object();


        public TDataObject this[TDataKey DataKey]
        {
            get => GetDataObject(DataKey);
        }

        public bool ContainsKey(TDataKey DataKey)
        {
            return GetDataObject(DataKey) != null;
        }

        public TDataObject GetDataObject(TDataKey DataKey)
        {
            lock (_DataObjectLock)
            {
                if (_DataLookup.TryGetValue(DataKey, out var dataObject))
                    return dataObject;
            }

            return null;
        }

        public int GetDataObjectIndex(TDataKey DataKey, out TDataObject DataObject)
        {
            lock (_DataObjectLock)
            {
                if (_DataLookup.TryGetValue(DataKey, out DataObject))
                    return _DataObjects.IndexOf(DataObject);
            }

            return -1;
        }

        public int GetDataObjectIndex(TDataKey DataKey)
        {
            return GetDataObjectIndex(DataKey, out _);
        }

        public IEnumerator<TDataObject> GetEnumerator()
        {
            //HACK: return a copy of the list so we don't get "Collection was modified;
            //      enumeration operation may not execute" as we cannot lock an Enumerator
            return _DataObjects.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int DataObjectCount => _DataObjects.Count;

        public int GetDataObjectIndex(TDataObject DataObject)
        {
            lock (_DataObjectLock)
            {
                return _DataObjects.IndexOf(DataObject);
            }
        }

        public TDataObject GetDataObjectByIndex(int index)
        {
            if ((index < 0) || (index >= DataObjectCount))
                return null;

            lock (_DataObjectLock)
            {
                return _DataObjects[index];
            }
        }

        protected virtual int InsertDataObject(TDataKey dataKey, int index, TDataObject DataObject)
        {
            if (DataObject == null)
                return -1;

            //Allow insert to after last item
            if ((index < 0) || (index > _DataObjects.Count))
                return -1;

            var existingDataObject = GetDataObject(dataKey);
            if (existingDataObject != null)
            {
                return ReplaceDataObject(dataKey, DataObject);
            }

            CheckGroupUpdate();
            
            lock (_DataObjectLock)
            {
                _DataObjects.Insert(index, DataObject);
                _DataLookup[dataKey] = DataObject;
                index = _DataObjects.IndexOf(DataObject);
            }

            OnDataChanged(DataModificationTypes.Added,
                DataObject, index);

            return index;
        }

        protected virtual int AddDataObject(TDataKey dataKey, TDataObject DataObject)
        {
            return InsertDataObject(dataKey, _DataObjects.Count, DataObject);
        }

        /// <summary>
        /// Helper function to Update Properties of Data at a given index
        /// </summary>
        protected bool UpdateDataObject(TDataKey dataKey, Func<TDataObject, string[]> UpdateAction)
        {
            var dataObject = GetDataObject(dataKey);
            if (dataObject == null)
                return false;

            return UpdateDataObject(GetDataObject(dataKey), UpdateAction(dataObject));
        }

        protected bool UpdateDataObject(TDataKey dataKey, string[] UpdatedProperties)
        {
            return UpdateDataObject(GetDataObject(dataKey), UpdatedProperties);
        }

        protected virtual bool UpdateDataObject(TDataObject dataObject, string[] UpdatedProperties)
        {
            int index;

            if (dataObject == null)
                return false;

            if (UpdatedProperties == null)
                return false;

            lock (_DataObjectLock)
            {
                index = _DataObjects.IndexOf(dataObject);
            }

            CheckGroupUpdate();

            OnDataChanged(DataModificationTypes.Updated,
                dataObject, index, UpdatedProperties);

            return true;
        }

        /// <summary>
        /// Helper function to replace Data at a given index with a new Data
        /// </summary>
        protected virtual int ReplaceDataObject(TDataKey dataKey, TDataObject DataObject)
        {
            if (DataObject == null)
                return -1;

            var existingDataObject = GetDataObject(dataKey);
            if (existingDataObject == null)
            {
                return AddDataObject(dataKey, DataObject);
            }

            int index;

            CheckGroupUpdate();

            lock (_DataObjectLock)
            {
                index = _DataObjects.IndexOf(DataObject);
                _DataObjects[index] = DataObject;
                _DataLookup[dataKey] = DataObject;
            }

            OnDataChanged(DataModificationTypes.Updated,
                DataObject, index);

            return index;
        }


        /// <summary>
        /// Helper function to remove data at a specified index
        /// </summary>
        protected virtual int RemoveDataObject(TDataKey dataKey)
        {
            var dataObject = GetDataObject(dataKey);
            if (dataObject == null)
                return -1;

            CheckGroupUpdate();

            int index;

            lock (_DataObjectLock)
            {
                index = _DataObjects.IndexOf(dataObject);
                _DataObjects.Remove(dataObject);
                _DataLookup.Remove(dataKey);
            }

            OnDataChanged(DataModificationTypes.Removed,
                dataObject, index);

            return index;
        }

        protected virtual void ClearDataObjects()
        {
            if (_DataObjects.Count > 0)
            {
                CheckGroupUpdate();

                lock (_DataObjectLock)
                {
                    _DataObjects.Clear();
                    _DataLookup.Clear();
                }

                OnDataChanged(DataModificationTypes.Cleared, default, -1);
            }
        }

        /// <summary>
        /// Helper function to remove data
        /// </summary>
        protected override void ClearData(bool disconnected)
        {
            ClearDataObjects();
        }

        protected override void StartMultiCommand()
        {
            OnDataChanged(DataModificationTypes.MultiCommandStart, default, -1);
        }

        protected override void EndMultiCommand()
        {
            OnDataChanged(DataModificationTypes.MultiCommandEnd, default, -1);
        }

        /* HACK: Group updates based on received socket data
         *
         * The server sends some commands as a multi command, and some as multiple
         * single commands. Since the server generally processes similar commands
         * one after another, when we receive a server packet containing multiple
         * messages, count it as a 'group' of commands.
         *
         * We then count the number of updates done in the group, and when we reach
         * a configured threshold, we notify the start of a 'group update'.
         *
         * This allows us to smartly call BeginUpdate()/EndUpdate() functions
         * on list controls to have all the individual updates done as a single
         * group.
         *
         * Alternatively, we could update the server to send the commands as
         * multicommands but this should work.
         */
        private void Handler_BeginParseBuffer(object sender, MessageHandler.ParseBufferEventArgs e)
        {
            StartGroupUpdate();
        }

        private void Handler_EndParseBuffer(object sender, MessageHandler.ParseBufferEventArgs e)
        {
            EndGroupUpdate();
        }

        /// <summary>
        /// Flag to indicate if we are currently in a group update
        /// </summary>
        public bool InGroupUpdate { get; private set; } = false;

        private int GroupUpdateCounter = -1;

        /// <summary>
        /// Number of Updates before triggering GroupUpdateStart
        /// </summary>
        public virtual int GroupUpdateStartThreshold { get; set; } = 1;

        /// <summary>
        /// Number of Updates before forcibly triggering GroupUpdateEnd then GroupUpdateStart
        /// </summary>
        public virtual int GroupUpdateForceRestartThreshold { get; set; } = 0;

        private void StartGroupUpdate()
        {
            EndGroupUpdate();

            GroupUpdateCounter = 0; //mark as group possible
        }

        private void CheckGroupUpdate()
        {
            if (GroupUpdateCounter != -1) //if group is possible or counting group updates
                GroupUpdateCounter++;

            if (!InGroupUpdate)
            {
                //if we've passed the configured threshold, then start a group update
                if (GroupUpdateCounter >= GroupUpdateStartThreshold)
                {
                    InGroupUpdate = true;

                    OnDataChanged(DataModificationTypes.GroupUpdateStart, default, -1);
                }
            }
            else //in group update
            {
                //if we've passed the configured threshold, then end a group update and restart
                //this allows an update or redraw to be forced
                if ((GroupUpdateForceRestartThreshold > GroupUpdateStartThreshold) &&
                    (GroupUpdateCounter >= GroupUpdateForceRestartThreshold))
                {
                    OnDataChanged(DataModificationTypes.GroupUpdateEnd, default, -1);
                    OnDataChanged(DataModificationTypes.GroupUpdateStart, default, -1);
                    GroupUpdateCounter = GroupUpdateStartThreshold;
                }
            }
        }

        private void EndGroupUpdate()
        {
            if (InGroupUpdate)
            {
                OnDataChanged(DataModificationTypes.GroupUpdateEnd, default, -1);

                InGroupUpdate = false;
                GroupUpdateCounter = -1; //mark as no group possible
            }
        }

        protected virtual void OnDataChanged(DataModificationTypes ModificationType, 
            TDataObject Data, int Index, string[] UpdatedProperties = null)
        {
            OnDataChanged(new DataListUpdatedEventArgs<TDataObject>()
            {
                Modification = ModificationType,
                Data = Data,
                Index = Index,
                UpdatedProperties = UpdatedProperties
            });
        }
    }
}

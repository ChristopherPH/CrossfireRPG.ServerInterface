using CrossfireRPG.ServerInterface.Network;
using CrossfireRPG.ServerInterface.Protocol;
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
    /// <typeparam name="TDataKey">Type of key, used for quick lookups of DataObjects</typeparam>
    /// <typeparam name="TDataObject">Type of DataObject</typeparam>
    /// <typeparam name="TDataList">Type of List of DataObjects (eg List, BindingList) </typeparam>
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


        private readonly Dictionary<TDataKey, TDataObject> _DataObjectCache = new Dictionary<TDataKey, TDataObject>();
        private readonly object _DataObjectLock = new object();

        /// <summary>
        /// Direct access to data objects. Note this is not locked.
        /// </summary>
        protected TDataList _DataObjects { get; } = new TDataList();

        public TDataObject this[TDataKey DataKey]
        {
            get => GetDataObject(DataKey);
        }

        /// <summary>
        /// Checks if the DataObject exists in cache with the specified key
        /// </summary>
        /// <returns>true if DataObject exists in the cache</returns>
        public bool ContainsKey(TDataKey DataKey)
        {
            lock (_DataObjectLock)
            {
                return _DataObjectCache.ContainsKey(DataKey);
            }
        }

        /// <summary>
        /// Gets the DataObject from cache with the specified key
        /// </summary>
        /// <param name="DataKey">Unique key used to look up the DataObject</param>
        /// <returns>DataObject if exists in the cache</returns>
        public TDataObject GetDataObject(TDataKey DataKey)
        {
            lock (_DataObjectLock)
            {
                if (_DataObjectCache.TryGetValue(DataKey, out var dataObject))
                    return dataObject;
            }

            return null;
        }

        /// <summary>
        /// Gets the DataObject from cache with the specified key, and returns the DataObject index
        /// Note: If the index of the DataObject is not required, then use GetDataObject(TDataKey) as
        ///       it is much faster.
        /// </summary>
        /// <param name="DataKey">Unique key used to look up the DataObject</param>
        /// <param name="DataObject">DataObject associated with the key</param>
        /// <param name="Index">Array Index of DataObject</param>
        /// <returns>true if DataObject if exists in the cache</returns>
        public bool GetDataObject(TDataKey DataKey, out TDataObject DataObject, out int Index)
        {
            lock (_DataObjectLock)
            {
                if (_DataObjectCache.TryGetValue(DataKey, out DataObject))
                {
                    Index = _DataObjects.IndexOf(DataObject);
                    return true;
                }
            }

            Index = -1;
            return false;
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

        /// <summary>
        /// Number of DataObjects in the DataListManager
        /// </summary>
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

            //check if key already exists
            var existingDataObject = GetDataObject(dataKey);

            if (existingDataObject != null) //key exists
            {
                return ReplaceDataObject(dataKey, DataObject);
            }

            //key doesn't exist, add key and object
            CheckGroupUpdate();
            
            lock (_DataObjectLock)
            {
                _DataObjectCache[dataKey] = DataObject;

                //Insert or add object, and then re-find the index.
                //The list might actually be a sorted list, and the
                //object position may not be where we expect.
                _DataObjects.Insert(index, DataObject);
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

        protected int UpdateDataObject(TDataKey dataKey, Action<TDataObject> UpdateDataObjectAction)
        {
            var dataObject = GetDataObject(dataKey);
            if (dataObject == null)
                return -1;

            dataObject.BeginPropertiesChanged();
            UpdateDataObjectAction(dataObject);

            return UpdateDataObject(GetDataObject(dataKey), dataObject.EndPropertiesChanged());
        }

        protected int UpdateDataObject(TDataKey dataKey, IEnumerable<string> UpdatedProperties)
        {
            return UpdateDataObject(GetDataObject(dataKey), UpdatedProperties);
        }

        /// <summary>
        /// Raises an Update notification with the given changed properties
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="UpdatedProperties"></param>
        /// <returns></returns>
        protected virtual int UpdateDataObject(TDataObject dataObject, IEnumerable<string> UpdatedProperties)
        {
            int index;

            if (dataObject == null)
                return -1;

            //require at least one property changed to trigger an update
            if ((UpdatedProperties == null) || (UpdatedProperties.Count() == 0))
                return -1;

            lock (_DataObjectLock)
            {
                index = _DataObjects.IndexOf(dataObject);
            }

            CheckGroupUpdate();

            OnDataChanged(DataModificationTypes.Updated,
                dataObject, index, UpdatedProperties);

            return index;
        }

        /// <summary>
        /// Helper function to replace Data at a given index with a new Data
        /// </summary>
        protected virtual int ReplaceDataObject(TDataKey dataKey, TDataObject DataObject)
        {
            if (DataObject == null)
                return -1;

            //check if key already exists
            var existingDataObject = GetDataObject(dataKey);

            if (existingDataObject == null) //key doesn't exist, add key and object
            {
                //Note: The added object will be appended to the TDataList
                //      since there is no way to tell where it should be inserted
                return AddDataObject(dataKey, DataObject);
            }

            //key exists, update data object
            int index;

            CheckGroupUpdate();

            lock (_DataObjectLock)
            {
                index = _DataObjects.IndexOf(existingDataObject);
                _DataObjects[index] = DataObject;
                _DataObjectCache[dataKey] = DataObject;
            }

            OnDataChanged(DataModificationTypes.Updated,
                DataObject, index);

            return index;
        }


        /// <summary>
        /// Removes a DataObject given a DataKey
        /// </summary>
        /// <param name="dataKey">Unique key used to look up the DataObject</param>
        /// <returns>index of DataObject removed</returns>
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
                _DataObjects.RemoveAt(index);
                _DataObjectCache.Remove(dataKey);
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
                    _DataObjectCache.Clear();
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
            TDataObject Data, int Index, IEnumerable<string> UpdatedProperties = null)
        {
            OnDataChanged(new DataListUpdatedEventArgs<TDataObject>()
            {
                Modification = ModificationType,
                Data = Data,
                Index = Index,
                UpdatedProperties = UpdatedProperties?.ToHashSet()
            });
        }
    }
}

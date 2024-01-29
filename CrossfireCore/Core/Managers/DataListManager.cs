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
    public abstract class DataListManager<TDataObject, TDataList> :
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

        private TDataList DataObjects { get; } = new TDataList();

        public TDataObject this[int index]
        {
            get
            {
                lock (_Lock)
                {
                    return DataObjects[index];
                }
            }
        }

        public int Count => DataObjects.Count;

        private object _Lock = new object();

        public IEnumerator<TDataObject> GetEnumerator()
        {
            lock (_Lock)
            {
                //HACK: return a copy of the list so we don't get "Collection was modified;
                //      enumeration operation may not execute"
                return DataObjects.ToList().GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_Lock)
            {
                //HACK: return a copy of the list so we don't get "Collection was modified;
                //      enumeration operation may not execute"
                return this.ToList().GetEnumerator();
            }
        }

        public bool Contains(Predicate<TDataObject> Match)
        {
            return DataObjects.FindIndex(Match) == -1 ? false : true;
        }

        public TDataObject GetData(int index)
        {
            if ((index < 0) || (index >= Count))
                return default;

            lock (_Lock)
            {
                return DataObjects[index];
            }
        }

        public TDataObject GetData(Predicate<TDataObject> Match)
        {
            var index = DataObjects.FindIndex(Match);
            if (index == -1)
                return default;

            lock (_Lock)
            {
                return DataObjects[index];
            }
        }

        public TDataObject GetData(Predicate<TDataObject> Match, out int Index)
        {
            Index = DataObjects.FindIndex(Match);
            if (Index == -1)
                return default;

            lock (_Lock)
            {
                return DataObjects[Index];
            }
        }

        public int GetIndex(TDataObject Data)
        {
            if (Data == null)
                return default;

            lock (_Lock)
            {
                return DataObjects.IndexOf(Data);
            }
        }

        public int GetIndex(Predicate<TDataObject> Match)
        {
            return DataObjects.FindIndex(Match);
        }

        public int GetIndex(Predicate<TDataObject> Match, out TDataObject Data)
        {
            var index = DataObjects.FindIndex(Match);

            if (index == -1)
            {
                Data = default;
            }
            else
            {
                lock (_Lock)
                {
                    Data = DataObjects[index];
                }
            }

            return index;
        }

        protected int AddData(TDataObject Data)
        {
            var index = DataObjects.Count;

            CheckGroupUpdate();

            lock (_Lock)
            {
                DataObjects.Add(Data);
            }

            OnDataChanged(DataModificationTypes.Added,
                Data, index);

            return index;
        }

        /// <summary>
        /// Helper function to Update Properties of Data given a match
        /// </summary>
        protected bool UpdateData(Predicate<TDataObject> Match, Func<TDataObject, string[]> UpdateAction)
        {
            return UpdateData(DataObjects.FindIndex(Match), UpdateAction);
        }

        /// <summary>
        /// Helper function to Update Properties of Data at a given index
        /// </summary>
        protected bool UpdateData(int index, Func<TDataObject, string[]> UpdateAction)
        {
            if ((index < 0) || (index >= Count))
                return false;

            if (UpdateAction == null)
                return false;

            TDataObject data;
            lock (_Lock)
            {
                data = DataObjects[index];
            }

            var UpdatedProperties = UpdateAction(data);
            if (UpdatedProperties == null)
                return false;

            CheckGroupUpdate();

            OnDataChanged(DataModificationTypes.Updated,
                data, index, UpdatedProperties);

            return true;
        }

        /// <summary>
        /// Helper function to replace Data at a given index with a new Data
        /// </summary>
        protected bool UpdateData(int index, TDataObject Data)
        {
            if ((index < 0) || (index >= Count))
                return false;

            if (Data == null)
                return false;

            CheckGroupUpdate();

            lock (_Lock)
            {
                DataObjects[index] = Data;
            }

            OnDataChanged(DataModificationTypes.Updated,
                Data, index);

            return true;
        }

        /// <summary>
        /// Helper function to remove data given a match
        /// </summary>
        /// <param name="Match"></param>
        /// <returns></returns>
        protected bool RemoveData(Predicate<TDataObject> Match)
        {
            return RemoveData(DataObjects.FindIndex(Match));
        }

        /// <summary>
        /// Helper function to remove data
        /// </summary>
        protected bool RemoveData(TDataObject Data)
        {
            var index = GetIndex(Data);
            if (index == -1)
                return false;

            return RemoveData(index);
        }

        /// <summary>
        /// Helper function to remove data at a specified index
        /// </summary>
        protected bool RemoveData(int index)
        {
            if ((index < 0) || (index >= Count))
                return false;

            CheckGroupUpdate();

            TDataObject data;

            lock (_Lock)
            {
                data = DataObjects[index];
            }

            OnDataChanged(DataModificationTypes.Removed,
                data, index);

            lock (_Lock)
            {
                DataObjects.RemoveAt(index);
            }
            return true;
        }

        /// <summary>
        /// Helper function to remove data
        /// </summary>
        protected override void ClearData(bool disconnected)
        {
            if (DataObjects.Count > 0)
            {
                CheckGroupUpdate();

                lock (_Lock)
                {
                    DataObjects.Clear();
                }

                OnDataChanged(DataModificationTypes.Cleared, default, -1);
            }
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

    public static class DataListManagerExtensions
    {
        /* HACK: IList<> doesn't have a FindIndex, but List<T> does.
         */
        public static int FindIndex<T>(this IList<T> source, Predicate<T> match)
        {
            for (int ix = 0; ix < source.Count; ix++)
            {
                if (match(source[ix]))
                {
                    return ix;
                }
            }

            return -1;
        }
    }
}

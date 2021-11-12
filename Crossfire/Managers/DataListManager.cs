using CrossfireCore.ServerInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.Managers
{
    public abstract class DataListManager<T> : DataManager<T>, IEnumerable<T>
    {
        public DataListManager(SocketConnection Connection, MessageBuilder Builder, MessageParser Parser)
            : base(Connection, Builder, Parser) { }
        public override ModificationTypes SupportedModificationTypes => 
            base.SupportedModificationTypes | ModificationTypes.Cleared;

        private List<T> Datas { get; } = new List<T>();

        public T this[int index]
        {
            get
            {
                lock (_Lock)
                {
                    return Datas[index];
                }
            }
        }

        public int Count => Datas.Count;

        private object _Lock = new object();

        public IEnumerator<T> GetEnumerator()
        {
            lock (_Lock)
            {
                //HACK: return a copy of the list so we don't get "Collection was modified;
                //      enumeration operation may not execute"
                return Datas.ToList().GetEnumerator();
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

        public bool Contains(Predicate<T> Match)
        {
            return Datas.FindIndex(Match) == -1 ? false : true;
        }

        public T GetData(int index)
        {
            if ((index < 0) || (index >= Count))
                return default;

            lock (_Lock)
            {
                return Datas[index];
            }
        }

        public T GetData(Predicate<T> Match)
        {
            var index = Datas.FindIndex(Match);
            if (index == -1)
                return default;

            lock (_Lock)
            {
                return Datas[index];
            }
        }

        public T GetData(Predicate<T> Match, out int Index)
        {
            Index = Datas.FindIndex(Match);
            if (Index == -1)
                return default;

            lock (_Lock)
            {
                return Datas[Index];
            }
        }

        public int GetIndex(T Data)
        {
            if (Data == null)
                return default;

            lock (_Lock)
            {
                return Datas.IndexOf(Data);
            }
        }

        public int GetIndex(Predicate<T> Match)
        {
            return Datas.FindIndex(Match);
        }

        public int GetIndex(Predicate<T> Match, out T Data)
        {
            var index = Datas.FindIndex(Match);

            if (index == -1)
            {
                Data = default;
            }
            else
            {
                lock (_Lock)
                {
                    Data = Datas[index];
                }
            }

            return index;
        }

        protected int AddData(T Data)
        {
            var index = Datas.Count;

            lock (_Lock)
            {
                Datas.Add(Data);
            }

            OnDataChanged(ModificationTypes.Added,
                Data, index);

            return index;
        }

        /// <summary>
        /// Updates Properties of Data given a match
        /// </summary>
        protected bool UpdateData(Predicate<T> Match, Func<T, string[]> UpdateAction)
        {
            return UpdateData(Datas.FindIndex(Match), UpdateAction);
        }

        /// <summary>
        /// Updates Properties of Data at a given index
        /// </summary>
        protected bool UpdateData(int index, Func<T, string[]> UpdateAction)
        {
            if ((index < 0) || (index >= Count))
                return false;

            if (UpdateAction == null)
                return false;

            T data;
            lock (_Lock)
            {
                data = Datas[index];
            }

            var UpdatedProperties = UpdateAction(data);
            if (UpdatedProperties == null)
                return false;

            OnDataChanged(ModificationTypes.Updated,
                data, index, UpdatedProperties);

            return true;
        }

        /// <summary>
        /// Replaces Data at a given index with a new Data
        /// </summary>
        protected bool UpdateData(int index, T Data)
        {
            if ((index < 0) || (index >= Count))
                return false;

            if (Data == null)
                return false;

            lock (_Lock)
            {
                Datas[index] = Data;
            }

            OnDataChanged(ModificationTypes.Updated,
                Data, index);

            return true;
        }

        protected bool RemoveData(Predicate<T> Match)
        {
            return RemoveData(Datas.FindIndex(Match));
        }

        protected bool RemoveData(T Data)
        {
            var index = GetIndex(Data);
            if (index == -1)
                return false;

            return RemoveData(index);
        }

        protected bool RemoveData(int index)
        {
            if ((index < 0) || (index >= Count))
                return false;

            T data;

            lock (_Lock)
            {
                data = Datas[index];
            }

            OnDataChanged(ModificationTypes.Removed,
                data, index);

            lock (_Lock)
            {
                Datas.RemoveAt(index);
            }
            return true;
        }

        protected override void ClearData()
        {
            if (Datas.Count > 0)
            {
                lock (_Lock)
                {
                    Datas.Clear();
                }

                OnDataChanged(ModificationTypes.Cleared, default, -1);
            }
        }

        protected override void StartBatch()
        {
            OnDataChanged(ModificationTypes.BatchStart, default, -1);
        }

        protected override void EndBatch()
        {
            OnDataChanged(ModificationTypes.BatchEnd, default, -1);
        }

        protected virtual void OnDataChanged(ModificationTypes ModificationType, 
            T Data, int Index, string[] UpdatedProperties = null)
        {
            OnDataChanged(new DataListUpdatedEventArgs()
            {
                Modification = ModificationType,
                Data = Data,
                Index = Index,
                UpdatedProperties = UpdatedProperties
            });
        }

        public class DataListUpdatedEventArgs : DataUpdatedEventArgs
        {
            public int Index { get; set; } = -1;
        }
    }
}

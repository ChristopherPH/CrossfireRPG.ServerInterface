using CrossfireCore.ServerInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.Managers
{
    public abstract class DataManager<T> : IEnumerable<T>
    {
        public DataManager(SocketConnection Connection, MessageBuilder Builder, MessageParser Parser)
        {
            this.Connection = Connection;
            this.Builder = Builder;
            this.Parser = Parser;

            Connection.OnStatusChanged += Connection_OnStatusChanged;
        }

        protected SocketConnection Connection { get; private set; }
        protected MessageBuilder Builder { get; private set; }
        protected MessageParser Parser { get; private set; }

        public event EventHandler<DataUpdatedEventArgs> DataChanged;

        private List<T> Datas { get; } = new List<T>();

        public T this[int index]
        {
            get => Datas[index];
        }

        public int Count => Datas.Count;

        public IEnumerator<T> GetEnumerator()
        {
            return Datas.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private void Connection_OnStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            if (e.Status == ConnectionStatuses.Disconnected)
                ClearData();
        }

        public T GetData(int index)
        {
            if ((index < 0) || (index >= Count))
                return default;

            return Datas[index];
        }

        public T GetData(Predicate<T> Match)
        {
            var index = Datas.FindIndex(Match);
            if (index == -1)
                return default;

            return Datas[index];
        }

        public T GetData(Predicate<T> Match, out int Index)
        {
            Index = Datas.FindIndex(Match);
            if (Index == -1)
                return default;

            return Datas[Index];
        }

        public int GetData(Predicate<T> Match, out T Data)
        {
            var index = Datas.FindIndex(Match);
            Data = index == -1 ? default : Datas[index];
            return index;
        }

        protected int AddData(T Data)
        {
            var index = Datas.Count;
            Datas.Add(Data);

            OnDataChanged(DataUpdatedEventArgs.ModificationTypes.Added,
                Data, index);

            return index;
        }

        protected bool UpdateData(Predicate<T> Match, Func<T, bool> UpdateAction, object UpdateType = null)
        {
            return UpdateData(Datas.FindIndex(Match), UpdateAction, UpdateType);
        }

        protected bool UpdateData(int index, Func<T, bool> UpdateAction, object UpdateType = null)
        {
            if ((index < 0) || (index >= Count))
                return false;

            if (UpdateAction == null)
                return false;

            var data = Datas[index];
            if (!UpdateAction(data))
                return false;

            OnDataChanged(DataUpdatedEventArgs.ModificationTypes.Updated,
                data, index, UpdateType);

            return true;
        }

        protected bool RemoveData(Predicate<T> Match)
        {
            return RemoveData(Datas.FindIndex(Match));
        }

        protected bool RemoveData(int index)
        {
            if ((index < 0) || (index >= Count))
                return false;

            var data = Datas[index];

            OnDataChanged(DataUpdatedEventArgs.ModificationTypes.Removed,
                data, index);

            Datas.RemoveAt(index);
            return true;
        }

        protected void ClearData()
        {
            Datas.Clear();
            OnDataChanged(DataUpdatedEventArgs.ModificationTypes.Cleared, default, -1);
        }

        protected void StartBatch()
        {
            OnDataChanged(DataUpdatedEventArgs.ModificationTypes.BatchStart, default, -1);
        }

        protected void EndBatch()
        {
            OnDataChanged(DataUpdatedEventArgs.ModificationTypes.BatchEnd, default, -1);
        }

        protected virtual void OnDataChanged(DataUpdatedEventArgs.ModificationTypes ModificationType, 
            T Data, int Index, object Update = null)
        {
            DataChanged?.Invoke(this, new DataUpdatedEventArgs()
            {
                Modification = ModificationType,
                Data = Data,
                Index = Index,
                Update = Update
            });
        }

        public class DataUpdatedEventArgs : EventArgs
        {
            public enum ModificationTypes
            {
                Added,
                Removed,
                Updated,
                Cleared,
                BatchStart,
                BatchEnd
            }

            public ModificationTypes Modification { get; set; }
            public T Data { get; set; } = default;
            public int Index { get; set; } = -1;
            public object Update { get; set; } = null;

            public override string ToString()
            {
                return string.Format("{0}[{1}]: {2}", Data.GetType().Name, Modification, Data);
            }
        }
    }
}

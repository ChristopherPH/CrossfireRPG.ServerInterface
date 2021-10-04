using CrossfireCore.ServerInterface;
using CrossfireCore.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.Managers
{
    public abstract class DataManager<T> where T : class
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

        public event EventHandler<DataUpdatedEventArgs> DataUpdated;

        public List<T> Datas { get; } = new List<T>();

        public T this[int index]
        {
            get => Datas[index];
        }

        private void Connection_OnStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            if (e.Status == ConnectionStatuses.Disconnected)
                ClearData();
        }

        protected abstract void ClearData();

        protected virtual void OnManagerUpdated(DataUpdatedEventArgs.ModificationTypes ChangeType, T Data, int Index)
        {
            DataUpdated?.Invoke(this, new DataUpdatedEventArgs()
            {
                Modification = ChangeType,
                Data = Data,
                Index = Index
            });
        }

        public class DataUpdatedEventArgs : EventArgs
        {
            public enum ModificationTypes
            {
                Added,
                Removed,
                Updated,
            }

            public ModificationTypes Modification { get; set; }
            public T Data { get; set; }
            public int Index { get; set; }

            public override string ToString()
            {
                return string.Format("{0}[{1}]: {2}", Data.GetType().Name, Modification, Data);
            }
        }
    }
}

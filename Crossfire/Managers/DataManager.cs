using CrossfireCore.ServerInterface;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.Managers
{
    public abstract class DataManager<T> : Manager
    {
        public DataManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            if (ClearDataOnConnectionDisconnect)
                Connection.OnStatusChanged += Connection_OnStatusChanged;

            if (ClearDataOnNewPlayer)
                Handler.Player += Handler_Player;
        }

        protected abstract bool ClearDataOnConnectionDisconnect { get; }
        protected abstract bool ClearDataOnNewPlayer { get; }
        protected abstract void ClearData();

        public virtual ModificationTypes SupportedModificationTypes => ModificationTypes.None;

        private void Connection_OnStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            if (e.Status == ConnectionStatuses.Disconnected)
                ClearData();
        }

        private void Handler_Player(object sender, MessageHandler.PlayerEventArgs e)
        {
            if (e.tag == 0)
                ClearData();
        }

        public event EventHandler<DataUpdatedEventArgs> DataChanged;

        protected virtual void OnDataChanged(ModificationTypes ModificationType,
            T Data, string[] UpdatedProperties = null)
        {
            DataChanged?.Invoke(this, new DataUpdatedEventArgs()
            {
                Modification = ModificationType,
                Data = Data,
                UpdatedProperties = UpdatedProperties
            });
        }

        protected virtual void OnDataChanged(DataUpdatedEventArgs dataUpdatedEventArgs)
        {
            DataChanged?.Invoke(this, dataUpdatedEventArgs);
        }

        protected virtual void OnPropertyChanged(T Data, string UpdatedProperty)
        {
            OnDataChanged(ModificationTypes.Updated, Data, new string[] { UpdatedProperty });
        }

        protected virtual void StartMultiCommand()
        {
            OnDataChanged(ModificationTypes.MultiCommandStart, default);
        }

        protected virtual void EndMultiCommand()
        {
            OnDataChanged(ModificationTypes.MultiCommandEnd, default);
        }


        [Flags]
        public enum ModificationTypes
        {
            None = 0x00,

            Added = 0x01,
            Updated = 0x02,
            Removed = 0x04,
            Cleared = 0x08,

            MultiCommandStart = 0x10,
            MultiCommandEnd = 0x20,
            GroupUpdateStart = 0x40,
            GroupUpdateEnd = 0x80,
        }

        public class DataUpdatedEventArgs : EventArgs
        {
            public ModificationTypes Modification { get; set; }
            public T Data { get; set; } = default;

            /// <summary>
            /// List of properties that were updated, null indicates all/unknown properties, so best to update everything
            /// </summary>
            public string[] UpdatedProperties { get; set; } = null;

            public bool WasUpdated(string Property)
            {
                return (UpdatedProperties == null) || UpdatedProperties.Contains(Property);
            }

            public override string ToString()
            {
                return string.Format("{0}[{1}]: {2}", Data.GetType().Name, Modification, Data);
            }
        }
    }
}

using CrossfireCore.ServerInterface;
using System;
using System.Linq;

namespace CrossfireCore.Managers
{
    /// <summary>
    /// Manager class that holds, manages and organizes data for a single object
    /// Managers are used to combine multiple server messages/events into a single object
    /// with less updates
    /// </summary>
    public abstract class DataObjectManager<T> : DataManager
    {
        public DataObjectManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler) { }

        /// <summary>
        /// Setup property to indicate what events the manager will trigger
        /// </summary>
        public virtual ModificationTypes SupportedModificationTypes => ModificationTypes.None;

        /// <summary>
        /// Event when any data for the managed object changes
        /// </summary>
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

        /// <summary>
        /// Helper function to trigger a Updated event
        /// </summary>
        protected virtual void OnPropertyChanged(T Data, string UpdatedProperty)
        {
            OnDataChanged(ModificationTypes.Updated, Data, new string[] { UpdatedProperty });
        }

        /// <summary>
        /// Helper function to trigger a MultiCommandStart event
        /// </summary>
        protected virtual void StartMultiCommand()
        {
            OnDataChanged(ModificationTypes.MultiCommandStart, default);
        }

        /// <summary>
        /// Helper function to trigger a MultiCommandEnd event
        /// </summary>
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

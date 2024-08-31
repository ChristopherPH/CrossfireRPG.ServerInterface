using CrossfireRPG.ServerInterface.Network;
using CrossfireRPG.ServerInterface.Protocol;
using System;

namespace CrossfireRPG.ServerInterface.Managers
{
    /// <summary>
    /// Manager class that holds, manages and organizes data for a single object
    /// Managers are used to combine multiple server messages/events into a single object
    /// with less updates
    /// </summary>
    public abstract class DataObjectManager<TDataObject> : DataManager
        where TDataObject : DataObject
    {
        public DataObjectManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler) { }

        /// <summary>
        /// Setup property to indicate what events the manager will trigger
        /// </summary>
        public virtual DataModificationTypes SupportedModificationTypes => DataModificationTypes.None;

        /// <summary>
        /// Event when any data for the managed object changes
        /// </summary>
        public event EventHandler<DataUpdatedEventArgs<TDataObject>> DataChanged;

        protected virtual void OnDataChanged(DataModificationTypes ModificationType,
            TDataObject Data, string[] UpdatedProperties = null)
        {
            DataChanged?.Invoke(this, new DataUpdatedEventArgs<TDataObject>()
            {
                Modification = ModificationType,
                Data = Data,
                UpdatedProperties = UpdatedProperties
            });
        }

        protected virtual void OnDataChanged(DataUpdatedEventArgs<TDataObject> dataUpdatedEventArgs)
        {
            DataChanged?.Invoke(this, dataUpdatedEventArgs);
        }

        /// <summary>
        /// Helper function to trigger a Updated event
        /// </summary>
        protected virtual void OnPropertyChanged(TDataObject Data, string UpdatedProperty)
        {
            OnDataChanged(DataModificationTypes.Updated, Data, new string[] { UpdatedProperty });
        }

        /// <summary>
        /// Helper function to trigger a MultiCommandStart event
        /// </summary>
        protected virtual void StartMultiCommand()
        {
            OnDataChanged(DataModificationTypes.MultiCommandStart, default);
        }

        /// <summary>
        /// Helper function to trigger a MultiCommandEnd event
        /// </summary>
        protected virtual void EndMultiCommand()
        {
            OnDataChanged(DataModificationTypes.MultiCommandEnd, default);
        }
    }
}

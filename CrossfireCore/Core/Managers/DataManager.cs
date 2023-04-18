﻿using CrossfireCore.ServerInterface;

namespace CrossfireCore.Managers
{
    /// <summary>
    /// Base class for a manager, used to hold, manage and organize data from the server
    /// that triggers a clear based on connection and player events
    /// </summary>
    public abstract class DataManager : Manager
    {
        public DataManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            if (ClearDataOnConnectionDisconnect)
                Connection.OnStatusChanged += Connection_OnStatusChanged;

            if (ClearDataOnNewPlayer)
                Handler.Player += Handler_Player;
        }

        /// <summary>
        /// Setup property to indicate that the manager should clear all of its data 
        /// when the socket disconnects
        /// </summary>
        protected abstract bool ClearDataOnConnectionDisconnect { get; }

        /// <summary>
        /// Setup property to indicate that the manager should clear all of its data 
        /// when the player changes
        /// </summary>
        protected abstract bool ClearDataOnNewPlayer { get; }

        /// <summary>
        /// Custom function that clears all the data for the manager
        /// </summary>
        protected abstract void ClearData();

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
    }
}
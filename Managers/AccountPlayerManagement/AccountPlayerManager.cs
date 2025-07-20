/*
 * Copyright (c) 2025 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using CrossfireRPG.ServerInterface.Network;
using CrossfireRPG.ServerInterface.Protocol;
using System;
using System.Collections.Generic;

namespace CrossfireRPG.ServerInterface.Managers.AccountPlayerManagement
{
    public class AccountPlayerManager : DataListManager<int, AccountPlayer, List<AccountPlayer>>
    {
        public AccountPlayerManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Handler.AccountPlayer += Handler_AccountPlayer;
        }

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => false;
        public override DataModificationTypes SupportedModificationTypes =>
            base.SupportedModificationTypes | DataModificationTypes.Added |
            DataModificationTypes.MultiCommandStart | DataModificationTypes.MultiCommandEnd;

        /// <summary>
        /// Number of players associated with the account.
        /// </summary>
        private int accountPlayerCount = -1;

        /// <summary>
        /// Raised when the account players have been populated.
        /// </summary>
        public event EventHandler<EventArgs> AccountPlayersPopulated;

        /// <summary>
        /// Flag indicating whether all account players have been populated.
        /// </summary>
        public bool HasAccountPlayers => accountPlayerCount == this.DataObjectCount;

        protected override void ClearData(bool disconnected)
        {
            base.ClearData(disconnected);

            accountPlayerCount = -1;
        }

        private void Handler_AccountPlayer(object sender, MessageHandler.AccountPlayerEventArgs e)
        {
            //PlayerNumber 0 indicates that the player list is starting to be populated.
            if (e.PlayerNumber == 0)
            {
                ClearDataObjects();

                accountPlayerCount = e.PlayerCount;

                if (accountPlayerCount == 0)
                    AccountPlayersPopulated?.Invoke(this, EventArgs.Empty);
            }
            else //Add player information
            {
                if (e.PlayerNumber == 1)
                    StartMultiCommand();

                AddDataObject(e.PlayerNumber, new AccountPlayer()
                {
                    PlayerNumber = e.PlayerNumber,
                    Level = e.Level,
                    FaceNumber = e.FaceNumber,
                    Name = e.Name,
                    Class = e.Class,
                    Race = e.Race,
                    Face = e.Face,
                    Party = e.Party,
                    Map = e.Map,
                });

                if (e.PlayerNumber == accountPlayerCount)
                {
                    EndMultiCommand();

                    AccountPlayersPopulated?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}

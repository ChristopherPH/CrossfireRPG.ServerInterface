/*
 * Copyright (c) 2024 Christopher Hayes
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using CrossfireRPG.ServerInterface.Definitions;
using CrossfireRPG.ServerInterface.Network;
using CrossfireRPG.ServerInterface.Protocol;
using System;
using TheBlackRoom.System.Extensions;

namespace CrossfireRPG.ServerInterface.Managers.MessageManagement
{
    public class MessageManager : Manager
    {
        public MessageManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Connection.OnError += Connection_OnError;
            Connection.OnStatusChanged += Connection_OnStatusChanged;
            Handler.Failure += Handler_Failure;
            Handler.DrawInfo += Handler_DrawInfo;
            Handler.DrawExtInfo += Handler_DrawExtInfo;
        }

        public event EventHandler<MessageInfo> Message;

        protected void OnMessage(MessageInfo messageInfo)
        {
            Message?.Invoke(this, messageInfo);
        }

        private void Connection_OnError(object sender, ConnectionErrorEventArgs e)
        {
            var s = string.Format("Connection: {0} {1}", e.ErrorCode, e.ErrorMessage);

            AddClientMessage(s, NewClient.MsgSubTypeClient.Error);
        }

        private void Connection_OnStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            var s = string.Format("{0} {1} {2}", e.Status.ToString(),
                e.Status == ConnectionStatuses.Disconnected ? "from" : "to",
                Connection.Host);

            AddClientMessage(s, NewClient.MsgSubTypeClient.Debug);
        }

        private void Handler_Failure(object sender, MessageHandler.FailureEventArgs e)
        {
            var s = string.Format("Failure: {0} {1}", e.ProtocolCommand, e.FailureString);

            AddClientMessage(s, NewClient.MsgSubTypeClient.Error);
        }

        private void Handler_DrawInfo(object sender, MessageHandler.DrawInfoEventArgs e)
        {
            OnMessage(new MessageInfo()
            {
                Colour = e.Color,
                Flags = 0,
                Message = e.Message,
                MessageType = NewClient.MsgTypes.None,
                SubType = NewClient.SubTypeNone
            });
        }

        private void Handler_DrawExtInfo(object sender, MessageHandler.DrawExtInfoEventArgs e)
        {
            //Split flags from colour to make handling easier
            var colour = (NewClient.NewDrawInfo)((int)e.Flags & NewClient.NewDrawInfoColorMask);
            var flags = (NewClient.NewDrawInfo)((int)e.Flags & ~NewClient.NewDrawInfoColorMask);

            OnMessage(new MessageInfo()
            {
                Colour = colour,
                Flags = flags,
                Message = e.Message,
                MessageType = e.MessageType,
                SubType = e.SubType
            });
        }

        public void AddClientMessage(string Message, NewClient.MsgSubTypeClient SubType)
        {
            OnMessage(new MessageInfo()
            {
                Colour = NewClient.NewDrawInfo.Black,
                Flags = 0,
                Message = Message,
                MessageType = NewClient.MsgTypes.Client,
                SubType = (int)SubType
            });
        }

        public static string GetMessageTypeDescription(NewClient.MsgTypes MessageType)
        {
            return MessageType.GetDescription();
        }

        public static string GetMessageTypeDescription(NewClient.MsgTypes MessageType, int SubType)
        {
            switch (MessageType)
            {
                case NewClient.MsgTypes.Book:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    if (Enum.IsDefined(typeof(NewClient.MsgSubTypeBook), SubType))
                        return ((NewClient.MsgSubTypeBook)SubType).GetDescription();
                    break;

                case NewClient.MsgTypes.Card:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    if (Enum.IsDefined(typeof(NewClient.MsgSubTypeCard), SubType))
                        return ((NewClient.MsgSubTypeCard)SubType).GetDescription();
                    break;

                case NewClient.MsgTypes.Paper:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    if (Enum.IsDefined(typeof(NewClient.MsgSubTypePaper), SubType))
                        return ((NewClient.MsgSubTypePaper)SubType).GetDescription();
                    break;

                case NewClient.MsgTypes.Sign:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    if (Enum.IsDefined(typeof(NewClient.MsgSubTypeSign), SubType))
                        return ((NewClient.MsgSubTypeSign)SubType).GetDescription();
                    break;

                case NewClient.MsgTypes.Monument:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    if (Enum.IsDefined(typeof(NewClient.MsgSubTypeMonument), SubType))
                        return ((NewClient.MsgSubTypeMonument)SubType).GetDescription();
                    break;

                case NewClient.MsgTypes.Dialog:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    if (Enum.IsDefined(typeof(NewClient.MsgSubTypeDialog), SubType))
                        return ((NewClient.MsgSubTypeDialog)SubType).GetDescription();
                    break;

                case NewClient.MsgTypes.MOTD:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    break;

                case NewClient.MsgTypes.Admin:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    if (Enum.IsDefined(typeof(NewClient.MsgSubTypeAdmin), SubType))
                        return ((NewClient.MsgSubTypeAdmin)SubType).GetDescription();
                    break;

                case NewClient.MsgTypes.Shop:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    if (Enum.IsDefined(typeof(NewClient.MsgSubTypeShop), SubType))
                        return ((NewClient.MsgSubTypeShop)SubType).GetDescription();
                    break;

                case NewClient.MsgTypes.Command:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    if (Enum.IsDefined(typeof(NewClient.MsgSubTypeCommand), SubType))
                        return ((NewClient.MsgSubTypeCommand)SubType).GetDescription();
                    break;

                case NewClient.MsgTypes.Attribute:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    if (Enum.IsDefined(typeof(NewClient.MsgSubTypeAttribute), SubType))
                        return ((NewClient.MsgSubTypeAttribute)SubType).GetDescription();
                    break;

                case NewClient.MsgTypes.Skill:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    if (Enum.IsDefined(typeof(NewClient.MsgSubTypeSkill), SubType))
                        return ((NewClient.MsgSubTypeSkill)SubType).GetDescription();
                    break;

                case NewClient.MsgTypes.Apply:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    if (Enum.IsDefined(typeof(NewClient.MsgSubTypeApply), SubType))
                        return ((NewClient.MsgSubTypeApply)SubType).GetDescription();
                    break;

                case NewClient.MsgTypes.Attack:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    if (Enum.IsDefined(typeof(NewClient.MsgSubTypeAttack), SubType))
                        return ((NewClient.MsgSubTypeAttack)SubType).GetDescription();
                    break;

                case NewClient.MsgTypes.Communication:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    if (Enum.IsDefined(typeof(NewClient.MsgSubTypeCommunication), SubType))
                        return ((NewClient.MsgSubTypeCommunication)SubType).GetDescription();
                    break;

                case NewClient.MsgTypes.Spell:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    if (Enum.IsDefined(typeof(NewClient.MsgSubTypeSpell), SubType))
                        return ((NewClient.MsgSubTypeSpell)SubType).GetDescription();
                    break;

                case NewClient.MsgTypes.Item:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    if (Enum.IsDefined(typeof(NewClient.MsgSubTypeItem), SubType))
                        return ((NewClient.MsgSubTypeItem)SubType).GetDescription();
                    break;

                case NewClient.MsgTypes.Misc:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    //HACK: knowledge_give() sends out Misc messages wtih a MSG_TYPE_CLIENT_NOTICE
                    if ((NewClient.MsgSubTypeClient)SubType == NewClient.MsgSubTypeClient.Notice)
                        return NewClient.MsgSubTypeClient.Notice.GetDescription();
                    break;

                case NewClient.MsgTypes.Victim:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    if (Enum.IsDefined(typeof(NewClient.MsgSubTypeVictim), SubType))
                        return ((NewClient.MsgSubTypeVictim)SubType).GetDescription();
                    break;

                case NewClient.MsgTypes.Client:
                    if (SubType == NewClient.SubTypeNone)
                        return "None";
                    if (Enum.IsDefined(typeof(NewClient.MsgSubTypeClient), SubType))
                        return ((NewClient.MsgSubTypeClient)SubType).GetDescription();
                    break;

                default:
                    return "Invalid MessageType: " + MessageType;
            }

            return string.Format("Invalid SubType {0} for MessageType {1}", 
                SubType, MessageType);
        }
    }

    public class MessageInfo : EventArgs
    {
        public NewClient.NewDrawInfo Colour { get; set; }
        public NewClient.NewDrawInfo Flags { get; set; }
        public NewClient.MsgTypes MessageType { get; set; }
        public int SubType { get; set; }
        public string Message { get; set; }
    }
}

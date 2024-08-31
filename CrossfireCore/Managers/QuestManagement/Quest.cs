﻿using System;

namespace CrossfireRPG.ServerInterface.Managers.QuestManagement
{
    public class Quest : DataObject
    {
        public UInt32 QuestID
        {
            get => _QuestID;
            set => SetProperty(ref _QuestID, value);
        }
        UInt32 _QuestID;

        public UInt32 ParentID
        {
            get => _ParentID;
            set => SetProperty(ref _ParentID, value);
        }
        UInt32 _ParentID;

        public string Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }
        string _Title;

        public Int32 Face
        {
            get => _Face;
            set => SetProperty(ref _Face, value);
        }
        Int32 _Face;

        public byte Replay
        {
            get => _Replay;
            set => SetProperty(ref _Replay, value);
        }
        byte _Replay;

        public byte End
        {
            get => _End;
            set => SetProperty(ref _End, value);
        }
        byte _End;

        public string Step
        {
            get => _Step;
            set => SetProperty(ref _Step, value);
        }
        string _Step;

        public override string ToString()
        {
            return string.Format("{0}:{1}: {2}", QuestID, ParentID, Title);
        }
    }
}

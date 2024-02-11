using System;

namespace CrossfireCore.Managers.KnowledgeManagement
{
    public class Knowledge : DataObject
    {
        public UInt32 KnowledgeID { get => _KnowledgeID; set => SetProperty(ref _KnowledgeID, value); }
        private UInt32 _KnowledgeID;

        public string Type { get => _Type; set => SetProperty(ref _Type, value); }
        private string _Type;

        public string Title { get => _Title; set => SetProperty(ref _Title, value); }
        private string _Title;

        public Int32 Face { get => _Face; set => SetProperty(ref _Face, value); }
        private Int32 _Face;


        public override string ToString()
        {
            return Title;
        }
    }
}

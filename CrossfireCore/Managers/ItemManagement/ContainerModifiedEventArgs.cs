using System;

namespace CrossfireRPG.ServerInterface.Managers.ItemManagement
{
    public class ContainerModifiedEventArgs : EventArgs
    {
        public enum ModificationTypes
        {
            Opened,
            Closed
        }

        public ModificationTypes Modification { get; set; }
        public Item Item { get; set; }
        public UInt32 ItemTag => Item.Tag;
        public int ItemIndex { get; set; }

        public override string ToString()
        {
            return string.Format("ContainerModified[{0}]: {1}", Modification, Item);
        }
    }
}

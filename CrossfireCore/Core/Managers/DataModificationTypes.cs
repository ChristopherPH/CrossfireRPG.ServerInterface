using System;

namespace CrossfireCore.Managers
{
    [Flags]
    public enum DataModificationTypes
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
}

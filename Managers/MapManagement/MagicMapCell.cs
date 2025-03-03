using System;
using System.Xml.Serialization;

namespace CrossfireRPG.ServerInterface.Managers.MapManagement
{
    public class MagicMapCell
    {
        public MagicMapCell() { }

        public MagicMapCell(byte magicMapInfo)
        {
            this.MagicMapInfo = magicMapInfo;
        }

        /// <summary>
        /// Magic Map Information
        /// </summary>
        [XmlIgnore]
        public byte MagicMapInfo { get; set; } = 0;

        /// <summary>
        /// Magic map cell colour
        /// </summary>
        public Definitions.NewClient.NewDrawInfo NDIColour =>
            (Definitions.NewClient.NewDrawInfo)(MagicMapInfo & Definitions.NewClient.FaceColourMask);

        /// <summary>
        /// Magic map cell flags
        /// </summary>
        public byte Flags => (byte)(MagicMapInfo & ~Definitions.NewClient.FaceColourMask);

        /// <summary>
        /// Flag indicating magic map cell is a wall cell
        /// </summary>
        public bool IsWall => (Flags & Definitions.NewClient.FaceWall) != 0;

        /// <summary>
        /// Flag indicating magic map cell is a floor cell
        /// </summary>
        [Obsolete("Server does not send correct floor flag", true)]
        public bool IsFloor => (Flags & Definitions.NewClient.FaceFloor) != 0;
    }
}

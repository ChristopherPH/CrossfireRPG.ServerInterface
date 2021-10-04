using CrossfireCore;
using CrossfireCore.ServerInterface;
using CrossfireCore.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.Managers
{
    public class MapManager : ManagerBase<MapInfo>
    {
        public MapManager(SocketConnection Connection, MessageBuilder Builder, MessageParser Parser) :
            base(Connection, Builder, Parser)
        {
            Parser.NewMap += Parser_NewMap;
            Parser.Map += Parser_Map;
            Parser.MapAnimation += Parser_MapAnimation;
            Parser.MapClear += Parser_MapClear;
            Parser.MapClearOld += Parser_MapClearOld;
            Parser.MapClearAnimationSmooth += Parser_MapClearAnimationSmooth;
            Parser.MapDarkness += Parser_MapDarkness;
            Parser.MapScroll += Parser_MapScroll;
            Parser.Smooth += Parser_Smooth;
            Parser.Tick += Parser_Tick;
        }

        static Logger _Logger = new Logger(nameof(MapManager));

        Point _mapScroll = new Point(0, 0);
        List<MapInfo> _mapData = new List<MapInfo>();

        public event EventHandler<MapUpdateEventArgs> MapUpdate;

        protected override void ClearData()
        {
            _mapScroll = new Point(0, 0);
            _mapData.Clear();
        }

        private void Parser_NewMap(object sender, EventArgs e)
        {
            ClearData();
        }

        private void Parser_Map(object sender, MessageParser.MapEventArgs e)
        {
            //TODO: verify scroll offset is required
            var mapX = e.X + _mapScroll.X;
            var mapY = e.Y + _mapScroll.Y;

            _mapData.RemoveAll(existing =>
                existing is MapFaceInfo mapInfo && mapInfo.X == mapX && mapInfo.Y == mapY && mapInfo.Layer == e.Layer);

            _mapData.Add(new MapFaceInfo()
            {
                X = mapX,
                Y = mapY,
                Layer = e.Layer,
                Face = e.Face,
                Smooth = e.Smooth,
            });
        }
    }

    public class MapUpdateEventArgs : EventArgs
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public abstract class MapInfo
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class MapLayerInfo : MapInfo
    {
        public int Layer { get; set; }
    }

    public class MapDarknessInfo : MapInfo
    {
        public byte Darkness { get; set; }
    }

    public class MapFaceInfo : MapLayerInfo
    {
        public int Face { get; set; }
        public int Smooth { get; set; }
    }

    public class MapAnimationInfo : MapLayerInfo
    {
        public UInt16 Animation { get; set; }
        public int AnimationType { get; set; }
        public byte AnimationSpeed { get; set; }
        public byte Smooth { get; set; }
    }
}

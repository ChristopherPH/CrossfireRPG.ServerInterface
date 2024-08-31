using CrossfireRPG.ServerInterface.Definitions;

namespace CrossfireRPG.ServerInterface.Managers.MapManagement
{
    public struct MapLabel
    {
        public NewClient.Map2Type_Label LabelType { get; set; }

        public string Label { get; set; }
    }
}

namespace CrossfireCore.ServerConfig
{
    public static class Map
    {
        public const int MAP_LAYERS = 10;

        public enum MapLayers
        {
            MAP_LAYER_FLOOR = 0,
            MAP_LAYER_NO_PICK1 = 1,     /* Non pickable ground objects */
            MAP_LAYER_NO_PICK2 = 2,     /* Non pickable ground objects */
            MAP_LAYER_ITEM1 = 3,        /* Items that can be picked up */
            MAP_LAYER_ITEM2 = 4,
            MAP_LAYER_ITEM3 = 5,
            MAP_LAYER_LIVING1 = 6,      /* Living creatures */
            MAP_LAYER_LIVING2 = 7,
            MAP_LAYER_FLY1 = 8,         /* Flying objects - creatures, spells */
            MAP_LAYER_FLY2 = 9,         /* Arrows, etc */
        }

        public enum AnimationTypes
        {
            Normal = 0,
            Randomize = 1,
            Synchronize = 2
        }
    }
}

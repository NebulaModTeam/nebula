namespace NebulaWorld.Planet
{
    public static class PlanetManager
    {
        public static bool EventFromServer { get; set; }
        public static bool EventFromClient { get; set; }

        public static void Initialize()
        {
            EventFromServer = false;
            EventFromClient = false;
        }
    }
}

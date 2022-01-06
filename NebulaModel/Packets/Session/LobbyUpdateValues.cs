namespace NebulaModel.Packets.Session
{
    public class LobbyUpdateValues
    {
        public int GalaxyAlgo { get; set; }
        public int GalaxySeed { get; set; }
        public int StarCount { get; set; }
        public float ResourceMultiplier { get; set; }
        public LobbyUpdateValues() { }
        public LobbyUpdateValues(int galaxyAlgo, int galaxySeed, int starCount, float resourceMultiplier)
        {
            GalaxyAlgo = galaxyAlgo;
            GalaxySeed = galaxySeed;
            StarCount = starCount;
            ResourceMultiplier = resourceMultiplier;
        }
    }
}

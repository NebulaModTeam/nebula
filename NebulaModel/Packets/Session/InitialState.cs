namespace NebulaModel.Packets.Session
{
    public class InitialState
    {
        public int AlgoVersion { get; set; }
        public int GalaxySeed { get; set; }
        public int StarCount { get; set; }
        public float ResourceMultiplier { get; set; }

        public InitialState() { }

        public InitialState(int algoVersion, int galaxySeed, int starCount, float resourceMultiplier)
        {
            AlgoVersion = algoVersion;
            GalaxySeed = galaxySeed;
            StarCount = starCount;
            ResourceMultiplier = resourceMultiplier;
        }
    }
}

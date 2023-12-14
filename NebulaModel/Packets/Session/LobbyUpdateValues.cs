namespace NebulaModel.Packets.Session;

public class LobbyUpdateValues
{
    public LobbyUpdateValues() { }

    public LobbyUpdateValues(int galaxyAlgo, int galaxySeed, int starCount, float resourceMultiplier, bool isSandboxMode)
    {
        GalaxyAlgo = galaxyAlgo;
        GalaxySeed = galaxySeed;
        StarCount = starCount;
        ResourceMultiplier = resourceMultiplier;
        IsSandboxMode = isSandboxMode;
    }

    public int GalaxyAlgo { get; }
    public int GalaxySeed { get; }
    public int StarCount { get; }
    public float ResourceMultiplier { get; }
    public bool IsSandboxMode { get; }
}

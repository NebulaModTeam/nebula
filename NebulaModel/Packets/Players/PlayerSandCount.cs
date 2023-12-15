namespace NebulaModel.Packets.Players;

public class PlayerSandCount
{
    public PlayerSandCount() { }

    public PlayerSandCount(int sandCount)
    {
        SandCount = sandCount;
    }

    public int SandCount { get; set; }
}

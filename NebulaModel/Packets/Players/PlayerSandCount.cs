namespace NebulaModel.Packets.Players;

public class PlayerSandCount
{
    public PlayerSandCount() { }

    public PlayerSandCount(long sandCount)
    {
        SandCount = sandCount;
    }

    public long SandCount { get; set; }
}

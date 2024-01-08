namespace NebulaModel.Packets.Players;

public class PlayerSandCount
{
    public PlayerSandCount() { }

    public PlayerSandCount(long sandCount, long sandChange = 0)
    {
        SandCount = sandCount;
        SandChange = sandChange;
    }

    public long SandCount { get; set; }
    public long SandChange { get; set; }
}

namespace NebulaModel.Packets.Players;

public class PlayerSandCount
{
    public PlayerSandCount() { }

    public PlayerSandCount(long sandCount, bool isDelta = false)
    {
        SandCount = sandCount;
        IsDelta = isDelta;
    }

    public long SandCount { get; set; }
    public bool IsDelta { get; set; }
}

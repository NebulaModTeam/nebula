namespace NebulaModel.Packets.Statistics;

public class MilestoneUnlockPacket
{
    public MilestoneUnlockPacket() { }

    public MilestoneUnlockPacket(int id, long unlockTick, int patternId, long[] parameters)
    {
        Id = id;
        UnlockTick = unlockTick;
        PatternId = patternId;
        Parameters = parameters;
    }

    public int Id { get; }
    public long UnlockTick { get; }
    public int PatternId { get; }
    public long[] Parameters { get; }
}

namespace NebulaModel.Packets.GameHistory;

public class GameHistoryTechRefundPacket
{
    public GameHistoryTechRefundPacket() { }

    public GameHistoryTechRefundPacket(int techId, long contributed)
    {
        TechIdContributed = techId;
        TechHashedContributed = contributed;
    }

    public int TechIdContributed { get; }
    public long TechHashedContributed { get; }
}

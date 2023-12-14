namespace NebulaModel.Packets.GameHistory;

public class GameHistoryResearchContributionPacket
{
    public GameHistoryResearchContributionPacket() { }

    public GameHistoryResearchContributionPacket(long hashes, int techId)
    {
        Hashes = hashes;
        TechId = techId;
    }

    public long Hashes { get; }
    public int TechId { get; }
}

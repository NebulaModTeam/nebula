namespace NebulaModel.Packets.GameHistory;

public class GameHistoryResearchUpdatePacket
{
    public GameHistoryResearchUpdatePacket() { }

    public GameHistoryResearchUpdatePacket(int techId, long hashUploaded, long hashNeeded, int techHashedFor10Frames)
    {
        TechId = techId;
        HashUploaded = hashUploaded;
        HashNeeded = hashNeeded;
        TechHashedFor10Frames = techHashedFor10Frames;
    }

    public int TechId { get; }
    public long HashUploaded { get; }
    public long HashNeeded { get; }
    public int TechHashedFor10Frames { get; }
}

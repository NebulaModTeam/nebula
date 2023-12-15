namespace NebulaModel.Packets.GameHistory;

public class GameHistoryEnqueueTechPacket
{
    public GameHistoryEnqueueTechPacket() { }

    public GameHistoryEnqueueTechPacket(int techId)
    {
        TechId = techId;
    }

    public int TechId { get; set; }
}

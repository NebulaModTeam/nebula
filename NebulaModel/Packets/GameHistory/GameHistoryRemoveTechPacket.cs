namespace NebulaModel.Packets.GameHistory;

public class GameHistoryRemoveTechPacket
{
    public GameHistoryRemoveTechPacket() { }

    public GameHistoryRemoveTechPacket(int techId)
    {
        TechId = techId;
    }

    public int TechId { get; set; }
}

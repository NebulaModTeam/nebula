namespace NebulaModel.Packets.GameHistory
{
    public class GameHistoryRemoveTechPacket
    {
        public int TechId { get; set; }

        public GameHistoryRemoveTechPacket() { }

        public GameHistoryRemoveTechPacket(int techId)
        {
            this.TechId = techId;
        }
    }
}
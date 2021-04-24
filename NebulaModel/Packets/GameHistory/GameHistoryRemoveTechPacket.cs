namespace NebulaModel.Packets.GameHistory
{
    public class GameHistoryRemoveTechPacket
    {
        public int techId { get; set; }

        public GameHistoryRemoveTechPacket() { }

        public GameHistoryRemoveTechPacket(int techId)
        {
            this.techId = techId;
        }
    }
}
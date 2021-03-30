namespace NebulaModel.Packets.GameHistory
{
    public class GameHistoryUnlockTechPacket
    {
        public int TechId { get; set; }

        public GameHistoryUnlockTechPacket() { }
        public GameHistoryUnlockTechPacket(int techId)
        {
            this.TechId = techId;
        }
    }
}

namespace NebulaModel.Packets.GameHistory
{
    public class GameHistoryUnlockTechPacket
    {
        public int TechId { get; set; }
        public int Level { get; set; }

        public GameHistoryUnlockTechPacket() { }
        public GameHistoryUnlockTechPacket(int techId, int level)
        {
            TechId = techId;
            Level = level;
        }
    }
}

namespace NebulaModel.Packets.GameHistory
{
    public class GameHistoryTechRefundPacket
    {
        public int TechIdContributed { get; set; }
        public long TechHashedContributed { get; set; }

        public GameHistoryTechRefundPacket() { }

        public GameHistoryTechRefundPacket(int techId, long contributed)
        {
            TechIdContributed = techId;
            TechHashedContributed = contributed;
        }
    }
}

namespace NebulaModel.Packets.GameHistory
{
    public class GameHistoryTechRefundPacket
    {
        public int TechIdContributed { get; set; }
        public long TechHashedContributed { get; set; }
        public int[] ItemIds { get; set; }

        public GameHistoryTechRefundPacket() { }

        public GameHistoryTechRefundPacket(int techId, int[] itemIds, long contributed)
        {
            this.TechIdContributed = techId;
            this.TechHashedContributed = contributed;
            this.ItemIds = itemIds;
        }
    }
}

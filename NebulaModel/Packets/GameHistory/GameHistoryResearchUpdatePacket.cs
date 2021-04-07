namespace NebulaModel.Packets.GameHistory
{
    public class GameHistoryResearchUpdatePacket
    {
        public int TechId { get; set; }
        public long HashUploaded { get; set; }

        public GameHistoryResearchUpdatePacket() { }

        public GameHistoryResearchUpdatePacket(int techId, long hashUploaded)
        {
            this.TechId = techId;
            this.HashUploaded = hashUploaded;
        }
    }
}

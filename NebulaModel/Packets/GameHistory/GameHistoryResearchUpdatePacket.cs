namespace NebulaModel.Packets.GameHistory
{
    public class GameHistoryResearchUpdatePacket
    {
        public int TechId { get; set; }
        public long HashUploaded { get; set; }
        public long HashNeeded { get; set; }
        public int TechHashedFor10Frames { get; set; }

        public GameHistoryResearchUpdatePacket() { }

        public GameHistoryResearchUpdatePacket(int techId, long hashUploaded, long hashNeeded, int techHashedFor10Frames)
        {
            TechId = techId;
            HashUploaded = hashUploaded;
            HashNeeded = hashNeeded;
            TechHashedFor10Frames = techHashedFor10Frames;
        }
    }
}

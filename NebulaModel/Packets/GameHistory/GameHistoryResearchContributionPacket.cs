namespace NebulaModel.Packets.GameHistory
{
    public class GameHistoryResearchContributionPacket
    {
        public long Hashes { get; set; }
        public int TechId { get; set; }

        public GameHistoryResearchContributionPacket() { }

        public GameHistoryResearchContributionPacket(long hashes, int techId)
        {
            Hashes = hashes;
            TechId = techId;
        }
    }
}

namespace NebulaModel.Packets.GameHistory
{
    public class GameHistoryEnqueueTechPacket
    {
        public int TechId { get; set; }

        public GameHistoryEnqueueTechPacket() { }
        public GameHistoryEnqueueTechPacket(int techId)
        {
            TechId = techId;
        }
    }
}

using System.Collections.Generic;

namespace NebulaModel.Packets.GameHistory
{
    public class GameHistoryTechRefundPacket
    {
        public int TechIdContributed { get; set; }
        public long TechHashedContributed { get; set; }

        public GameHistoryTechRefundPacket() { }

        public GameHistoryTechRefundPacket(int techId, long contributed)
        { 
            this.TechIdContributed = techId;
            this.TechHashedContributed = contributed;
        }
    }
}

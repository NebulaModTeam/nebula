using NebulaAPI;

namespace NebulaModel.Packets.Players
{
    [HidePacketInDebugLogs]
    public class PlayerResearchHashRateChanged
    {
        public int ResearchHashRate { get; set; }

        public PlayerResearchHashRateChanged() { }
        public PlayerResearchHashRateChanged(int hashRate)
        {
            ResearchHashRate = hashRate;
        }
    }
}

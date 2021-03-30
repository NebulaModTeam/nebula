using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Processors;

namespace NebulaHost.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryResearchContributionProcessor : IPacketProcessor<GameHistoryResearchContributionPacket>
    {
        public void ProcessPacket(GameHistoryResearchContributionPacket packet, NebulaConnection conn)
        {
            //Check if client is contributing to the correct Tech Research
            if (packet.TechId == GameMain.history.currentTech)
            {
                GameMain.history.AddTechHash(packet.Hashes);
            }
        }
    }
}

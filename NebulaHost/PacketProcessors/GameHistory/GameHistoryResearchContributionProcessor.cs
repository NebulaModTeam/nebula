using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Processors;
using NebulaModel.Logger;

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
                Log.Info($"ProcessPacket researchContribution: got package for same tech");
                GameMain.history.AddTechHash(packet.Hashes);
                PlayerManager playerManager = MultiplayerHostSession.Instance.PlayerManager;
                playerManager.GetPlayer(conn).UpdateResearchProgress(packet.TechId, packet.Hashes);
                Log.Debug($"ProcessPacket researchContribution: playerid by: {playerManager.GetPlayer(conn).Id} - hashes {packet.Hashes}");
            }
        }
    }
}


using NebulaModel.Attributes;
using NebulaModel.Logger;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;

namespace NebulaNetwork.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryResearchContributionProcessor : PacketProcessor<GameHistoryResearchContributionPacket>
    {
        public override void ProcessPacket(GameHistoryResearchContributionPacket packet, NetworkConnection conn)
        {
            if (IsClient) return;

            //Check if client is contributing to the correct Tech Research
            if (packet.TechId == GameMain.history.currentTech)
            {
                Log.Info($"ProcessPacket researchContribution: got package for same tech");
                GameMain.history.AddTechHash(packet.Hashes);
                PlayerManager playerManager = MultiplayerHostSession.Instance != null ? MultiplayerHostSession.Instance.PlayerManager : null;
                playerManager.GetPlayer(conn).UpdateResearchProgress(packet.TechId, packet.Hashes);
                Log.Debug($"ProcessPacket researchContribution: playerid by: {playerManager.GetPlayer(conn).Id} - hashes {packet.Hashes}");
            }
        }
    }
}


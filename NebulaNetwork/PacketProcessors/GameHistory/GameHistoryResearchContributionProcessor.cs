using NebulaAPI;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    internal class GameHistoryResearchContributionProcessor : PacketProcessor<GameHistoryResearchContributionPacket>
    {
        public override void ProcessPacket(GameHistoryResearchContributionPacket packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                return;
            }

            //Check if client is contributing to the correct Tech Research
            if (packet.TechId == GameMain.history.currentTech)
            {
                GameMain.history.AddTechHash(packet.Hashes);
                IPlayerManager playerManager = Multiplayer.Session.Network.PlayerManager;
                Log.Debug($"ProcessPacket researchContribution: playerid by: {playerManager.GetPlayer(conn).Id} - hashes {packet.Hashes}");
            }
            else
            {
                Log.Info($"ProcessPacket researchContribution: got package for different tech ({packet.TechId})");
            }
        }
    }
}


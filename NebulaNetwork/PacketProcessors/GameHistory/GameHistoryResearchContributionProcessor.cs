#region

using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;

#endregion

namespace NebulaNetwork.PacketProcessors.GameHistory;

[RegisterPacketProcessor]
internal class GameHistoryResearchContributionProcessor : PacketProcessor<GameHistoryResearchContributionPacket>
{
    protected override void ProcessPacket(GameHistoryResearchContributionPacket packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            return;
        }

        //Check if client is contributing to the correct Tech Research
        if (packet.TechId == GameMain.history.currentTech)
        {
            GameMain.history.AddTechHash(packet.Hashes);
        }
        else
        {
            Log.Info($"ProcessPacket researchContribution: got package for different tech ({packet.TechId})");
        }
    }
}

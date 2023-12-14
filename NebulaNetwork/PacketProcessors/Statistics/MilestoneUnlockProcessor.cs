#region

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Statistics;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Statistics;

[RegisterPacketProcessor]
internal class MilestoneUnlockProcessor : PacketProcessor<MilestoneUnlockPacket>
{
    public override void ProcessPacket(MilestoneUnlockPacket packet, NebulaConnection conn)
    {
        IPlayerManager playerManager = Multiplayer.Session.Network.PlayerManager;
        var valid = true;

        if (IsHost)
        {
            var player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
            else
            {
                valid = false;
            }
        }

        if (valid)
        {
            using (Multiplayer.Session.Statistics.IsIncomingRequest.On())
            {
                if (GameMain.data.milestoneSystem.milestoneDatas.TryGetValue(packet.Id, out var milestoneData))
                {
                    milestoneData.journalData.patternId = packet.PatternId;
                    milestoneData.journalData.parameters = packet.Parameters;
                    GameMain.data.milestoneSystem.UnlockMilestone(packet.Id, packet.UnlockTick);
                }
            }
        }
    }
}

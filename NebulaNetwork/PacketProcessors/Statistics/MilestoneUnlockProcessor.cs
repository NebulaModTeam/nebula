#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Statistics;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Statistics;

[RegisterPacketProcessor]
internal class MilestoneUnlockProcessor : PacketProcessor<MilestoneUnlockPacket>
{
    protected override void ProcessPacket(MilestoneUnlockPacket packet, NebulaConnection conn)
    {
        var valid = true;

        if (IsHost)
        {
            var player = Players.Get(conn);
            if (player != null)
            {
                Server.SendPacketExclude(packet, conn);
            }
            else
            {
                valid = false;
            }
        }

        if (!valid)
        {
            return;
        }
        using (Multiplayer.Session.Statistics.IsIncomingRequest.On())
        {
            if (!GameMain.data.milestoneSystem.milestoneDatas.TryGetValue(packet.Id, out var milestoneData))
            {
                return;
            }
            milestoneData.journalData.patternId = packet.PatternId;
            milestoneData.journalData.parameters = packet.Parameters;
            GameMain.data.milestoneSystem.UnlockMilestone(packet.Id, packet.UnlockTick);
        }
    }
}

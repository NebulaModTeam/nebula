#region

using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Statistics;

#endregion

namespace NebulaNetwork.PacketProcessors.Statistics;

[RegisterPacketProcessor]
internal class MilestoneDataResponseProcessor : PacketProcessor<MilestoneDataResponse>
{
    public override void ProcessPacket(MilestoneDataResponse packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            return;
        }

        //Reset all current values
        GameMain.data.milestoneSystem.Init(GameMain.data);

        Log.Info("Parsing Milstone data from the server.");
        using (var reader = new BinaryUtils.Reader(packet.BinaryData))
        {
            GameMain.data.milestoneSystem.Import(reader.BinaryReader);
        }
    }
}

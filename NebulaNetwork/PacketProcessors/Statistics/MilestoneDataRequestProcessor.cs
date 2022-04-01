using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Statistics;

namespace NebulaNetwork.PacketProcessors.Statistics
{
    [RegisterPacketProcessor]
    public class MilestoneDataRequestProcessor : PacketProcessor<MilestoneDataRequest>
    {
        public override void ProcessPacket(MilestoneDataRequest packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                return;
            }

            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                GameMain.data.milestoneSystem.Export(writer.BinaryWriter);
                conn.SendPacket(new MilestoneDataResponse(writer.CloseAndGetBytes()));
            }
        }
    }
}

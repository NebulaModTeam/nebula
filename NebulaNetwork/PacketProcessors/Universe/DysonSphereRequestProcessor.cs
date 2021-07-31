using NebulaModel.Attributes;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaModel.Networking;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    public class DysonSphereRequestProcessor : PacketProcessor<DysonSphereLoadRequest>
    {
        public override void ProcessPacket(DysonSphereLoadRequest packet, NetworkConnection conn)
        {
            if (IsClient) return;

            DysonSphere dysonSphere = GameMain.data.CreateDysonSphere(packet.StarIndex);

            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                dysonSphere.Export(writer.BinaryWriter);
                conn.SendPacket(new DysonSphereData(packet.StarIndex, writer.CloseAndGetBytes()));
            }
        }
    }
}

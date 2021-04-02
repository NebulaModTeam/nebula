using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Universe;

namespace NebulaHost.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    public class DysonSphereRequestProcessor : IPacketProcessor<DysonSphereLoadRequest>
    {
        public void ProcessPacket(DysonSphereLoadRequest packet, NebulaConnection conn)
        {
            DysonSphere dysonSphere = GameMain.data.CreateDysonSphere(packet.StarIndex);

            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                dysonSphere.Export(writer.BinaryWriter);
                conn.SendPacket(new DysonSphereData(packet.StarIndex, writer.CloseAndGetBytes()));
            }
        }
    }
}

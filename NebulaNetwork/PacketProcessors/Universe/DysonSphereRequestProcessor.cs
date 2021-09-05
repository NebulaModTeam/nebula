using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    public class DysonSphereRequestProcessor : PacketProcessor<DysonSphereLoadRequest>
    {
        public override void ProcessPacket(DysonSphereLoadRequest packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                return;
            }

            DysonSphere dysonSphere = GameMain.data.CreateDysonSphere(packet.StarIndex);

            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                dysonSphere.Export(writer.BinaryWriter);
                conn.SendPacket(new DysonSphereData(packet.StarIndex, writer.CloseAndGetBytes()));
            }
        }
    }
}

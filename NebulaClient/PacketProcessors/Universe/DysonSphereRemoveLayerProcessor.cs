using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaClient.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereRemoveLayerProcessor : PacketProcessor<DysonSphereRemoveLayerPacket>
    {
        public override void ProcessPacket(DysonSphereRemoveLayerPacket packet, NebulaConnection conn)
        {
            using (DysonSphere_Manager.IncomingDysonSpherePacket.On())
                GameMain.data.dysonSpheres[packet.StarIndex]?.RemoveLayer(packet.LayerId);
        }
    }
}

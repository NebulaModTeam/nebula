using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaClient.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereRemoveLayerProcessor : IPacketProcessor<DysonSphereRemoveLayerPacket>
    {
        public void ProcessPacket(DysonSphereRemoveLayerPacket packet, NebulaConnection conn)
        {
            using (DysonSphere_Manager.IncomingDysonSpherePacket.On())
                GameMain.data.dysonSpheres[packet.StarIndex]?.RemoveLayer(packet.LayerId);
        }
    }
}

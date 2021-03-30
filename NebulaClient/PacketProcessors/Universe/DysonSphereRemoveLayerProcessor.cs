using NebulaModel.Attributes;
using NebulaModel.Logger;
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
            Log.Info($"Processing DysonSphere Remove Layer notification for system {GameMain.data.galaxy.stars[packet.StarIndex].name} (Index: {GameMain.data.galaxy.stars[packet.StarIndex].index})");
            DysonSphere_Manager.IncomingDysonSpherePacket = true;
            GameMain.data.dysonSpheres[packet.StarIndex]?.RemoveLayer(packet.LayerId);
            DysonSphere_Manager.IncomingDysonSpherePacket = false;
        }
    }
}

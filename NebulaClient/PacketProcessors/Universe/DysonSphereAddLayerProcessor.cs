using NebulaModel.Attributes;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaClient.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    public class DysonSphereAddLayerProcessor : IPacketProcessor<DysonSphereAddLayerPacket>
    {
        public static bool IncomingPacket = false;

        public void ProcessPacket(DysonSphereAddLayerPacket packet, NebulaConnection conn)
        {
            Log.Info($"Processing DysonSphere Add Layer notification for system {GameMain.data.galaxy.stars[packet.StarIndex].name} (Index: {GameMain.data.galaxy.stars[packet.StarIndex].index})");
            using (DysonSphere_Manager.IncomingDysonSpherePacket.On())
            {
                GameMain.data.dysonSpheres[packet.StarIndex]?.AddLayer(packet.OrbitRadius, DataStructureExtensions.ToQuaternion(packet.OrbitRotation), packet.OrbitAngularSpeed);
            }
        }
    }
}

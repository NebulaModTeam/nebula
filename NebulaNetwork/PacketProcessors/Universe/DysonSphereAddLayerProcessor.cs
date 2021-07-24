using NebulaModel.Attributes;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    public class DysonSphereAddLayerProcessor : PacketProcessor<DysonSphereAddLayerPacket>
    {
        public static bool IncomingPacket = false;

        public override void ProcessPacket(DysonSphereAddLayerPacket packet, NebulaConnection conn)
        {
            using (DysonSphere_Manager.IncomingDysonSpherePacket.On())
            {
                GameMain.data.dysonSpheres[packet.StarIndex]?.AddLayer(packet.OrbitRadius, DataStructureExtensions.ToQuaternion(packet.OrbitRotation), packet.OrbitAngularSpeed);
            }
        }
    }
}

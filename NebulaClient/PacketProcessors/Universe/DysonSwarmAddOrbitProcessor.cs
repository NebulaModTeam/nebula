using NebulaModel.Attributes;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaClient.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSwarmAddOrbitProcessor : IPacketProcessor<DysonSwarmAddOrbitPacket>
    {
        public void ProcessPacket(DysonSwarmAddOrbitPacket packet, NebulaConnection conn)
        {
            using (DysonSphere_Manager.IncomingDysonSwarmPacket.On())
            {
                GameMain.data.dysonSpheres[packet.StarIndex]?.swarm?.NewOrbit(packet.Radius, DataStructureExtensions.ToQuaternion(packet.Rotation));
            }
        }
    }
}

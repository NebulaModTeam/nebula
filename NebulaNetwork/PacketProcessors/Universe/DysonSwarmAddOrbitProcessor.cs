using NebulaModel.Attributes;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSwarmAddOrbitProcessor : PacketProcessor<DysonSwarmAddOrbitPacket>
    {
        public override void ProcessPacket(DysonSwarmAddOrbitPacket packet, NebulaConnection conn)
        {
            using (DysonSphere_Manager.IncomingDysonSwarmPacket.On())
            {
                GameMain.data.dysonSpheres[packet.StarIndex]?.swarm?.NewOrbit(packet.Radius, DataStructureExtensions.ToQuaternion(packet.Rotation));
            }
        }
    }
}

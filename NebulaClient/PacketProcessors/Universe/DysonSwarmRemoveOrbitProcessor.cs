using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaClient.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSwarmRemoveOrbitProcessor : IPacketProcessor<DysonSwarmRemoveOrbitPacket>
    {
        public void ProcessPacket(DysonSwarmRemoveOrbitPacket packet, NebulaConnection conn)
        {
            using (DysonSphere_Manager.IncomingDysonSwarmPacket.On())
            {
                GameMain.data.dysonSpheres[packet.StarIndex]?.swarm?.RemoveOrbit(packet.OrbitId);
            }
        }
    }
}

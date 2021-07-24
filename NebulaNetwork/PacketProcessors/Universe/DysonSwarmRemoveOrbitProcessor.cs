using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSwarmRemoveOrbitProcessor : PacketProcessor<DysonSwarmRemoveOrbitPacket>
    {
        public override void ProcessPacket(DysonSwarmRemoveOrbitPacket packet, NebulaConnection conn)
        {
            using (DysonSphere_Manager.IncomingDysonSwarmPacket.On())
            {
                GameMain.data.dysonSpheres[packet.StarIndex]?.swarm?.RemoveOrbit(packet.OrbitId);
            }
        }
    }
}

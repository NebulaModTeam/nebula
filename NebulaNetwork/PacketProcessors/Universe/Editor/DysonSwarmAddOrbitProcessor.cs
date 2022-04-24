using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    internal class DysonSwarmAddOrbitProcessor : PacketProcessor<DysonSwarmAddOrbitPacket>
    {
        public override void ProcessPacket(DysonSwarmAddOrbitPacket packet, NebulaConnection conn)
        {
            DysonSphere sphere = GameMain.data.dysonSpheres[packet.StarIndex];
            if (sphere == null)
            {
                return;
            }
            using (Multiplayer.Session.DysonSpheres.IncomingDysonSwarmPacket.On())
            {
                if (packet.OrbitId != NebulaWorld.Universe.DysonSphereManager.QueryOrbitId(sphere.swarm))
                {
                    Multiplayer.Session.DysonSpheres.HandleDesync(packet.StarIndex, conn);
                    return;
                }
                sphere.swarm.NewOrbit(packet.Radius, packet.Rotation.ToQuaternion());
            }
            if (IsHost)
            {
                Multiplayer.Session.DysonSpheres.SendPacketToDysonSphereExcept(packet, packet.StarIndex, conn);
            }
        }
    }
}

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    internal class DysonSwarmEditOrbitProcessor : PacketProcessor<DysonSwarmEditOrbitPacket>
    {
        public override void ProcessPacket(DysonSwarmEditOrbitPacket packet, NebulaConnection conn)
        {
            DysonSphere sphere = GameMain.data.dysonSpheres[packet.StarIndex];
            if (sphere == null)
            {
                return;
            }
            using (Multiplayer.Session.DysonSpheres.IncomingDysonSwarmPacket.On())
            {
                if (!sphere.swarm.OrbitExist(packet.OrbitId))
                {
                    Multiplayer.Session.DysonSpheres.HandleDesync(packet.StarIndex, conn);
                    return;
                }
                sphere.swarm.EditOrbit(packet.OrbitId, packet.Radius, packet.Rotation.ToQuaternion());
            }
            if (IsHost)
            {
                Multiplayer.Session.DysonSpheres.SendPacketToDysonSphereExcept(packet, packet.StarIndex, conn);
            }
        }
    }
}

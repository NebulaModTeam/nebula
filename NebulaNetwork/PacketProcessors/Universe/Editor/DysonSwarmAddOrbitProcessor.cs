#region

using NebulaAPI.DataStructures;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe.Editor;
using NebulaWorld;
using NebulaWorld.Universe;

#endregion

namespace NebulaNetwork.PacketProcessors.Universe.Editor;

[RegisterPacketProcessor]
internal class DysonSwarmAddOrbitProcessor : PacketProcessor<DysonSwarmAddOrbitPacket>
{
    protected override void ProcessPacket(DysonSwarmAddOrbitPacket packet, NebulaConnection conn)
    {
        var sphere = GameMain.data.dysonSpheres[packet.StarIndex];
        if (sphere == null)
        {
            return;
        }
        using (Multiplayer.Session.DysonSpheres.IncomingDysonSwarmPacket.On())
        {
            if (packet.OrbitId != DysonSphereManager.QueryOrbitId(sphere.swarm))
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

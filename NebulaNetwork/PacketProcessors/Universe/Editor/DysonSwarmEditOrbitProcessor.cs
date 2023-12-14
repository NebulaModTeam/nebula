#region

using NebulaAPI.DataStructures;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe.Editor;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaNetwork.PacketProcessors.Universe.Editor;

[RegisterPacketProcessor]
internal class DysonSwarmEditOrbitProcessor : PacketProcessor<DysonSwarmEditOrbitPacket>
{
    protected override void ProcessPacket(DysonSwarmEditOrbitPacket packet, NebulaConnection conn)
    {
        var sphere = GameMain.data.dysonSpheres[packet.StarIndex];
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
            if (packet.Radius >= 0)
            {
                sphere.swarm.EditOrbit(packet.OrbitId, packet.Radius, packet.Rotation.ToQuaternion());
            }
            else
            {
                sphere.swarm.SetOrbitColor(packet.OrbitId,
                    new Vector4(packet.Color.x, packet.Color.y, packet.Color.z, packet.Color.w));
            }
        }
        if (IsHost)
        {
            Multiplayer.Session.DysonSpheres.SendPacketToDysonSphereExcept(packet, packet.StarIndex, conn);
        }
    }
}

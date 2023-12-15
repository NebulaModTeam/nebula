#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Universe;

[RegisterPacketProcessor]
internal class DysonSailDataProcessor : PacketProcessor<DysonSailDataPacket>
{
    protected override void ProcessPacket(DysonSailDataPacket packet, NebulaConnection conn)
    {
        var dysonSphere = GameMain.data.dysonSpheres[packet.StarIndex];
        if (dysonSphere == null)
        {
            return;
        }
        using (Multiplayer.Session.DysonSpheres.IncomingDysonSwarmPacket.On())
        {
            DysonSail ss = default;
            ss.px = packet.px;
            ss.py = packet.py;
            ss.pz = packet.pz;
            ss.vx = packet.vx;
            ss.vy = packet.vy;
            ss.vz = packet.vz;
            ss.gs = packet.gs;
            dysonSphere.swarm.AddSolarSail(ss, packet.OrbitId, packet.ExpiryTime);
        }
        if (IsHost)
        {
            Multiplayer.Session.DysonSpheres.SendPacketToDysonSphereExcept(packet, packet.StarIndex, conn);
        }
    }
}

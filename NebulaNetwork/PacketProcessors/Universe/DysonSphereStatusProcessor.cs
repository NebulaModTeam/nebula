#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;

#endregion

namespace NebulaNetwork.PacketProcessors.Universe;

[RegisterPacketProcessor]
internal class DysonSphereStatusProcessor : PacketProcessor<DysonSphereStatusPacket>
{
    protected override void ProcessPacket(DysonSphereStatusPacket packet, NebulaConnection conn)
    {
        var dysonSphere = GameMain.data.dysonSpheres[packet.StarIndex];
        if (IsHost || dysonSphere == null)
        {
            return;
        }
        dysonSphere.grossRadius = packet.GrossRadius;
        dysonSphere.energyReqCurrentTick = packet.EnergyReqCurrentTick;
        dysonSphere.energyGenCurrentTick = packet.EnergyGenCurrentTick;
    }
}

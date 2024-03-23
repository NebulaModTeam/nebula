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
        // Replace some values set in DysonSphere.BeforeGameTick
        dysonSphere.grossRadius = packet.GrossRadius;
        dysonSphere.energyReqCurrentTick = packet.EnergyReqCurrentTick;
        dysonSphere.energyGenCurrentTick = packet.EnergyGenCurrentTick;
        dysonSphere.energyGenOriginalCurrentTick = (long)(dysonSphere.energyGenCurrentTick / dysonSphere.energyDFHivesDebuffCoef);
    }
}

#region

using NebulaAPI.Packets;

#endregion

namespace NebulaModel.Packets.Universe;

[HidePacketInDebugLogs]
public class DysonSphereStatusPacket
{
    public DysonSphereStatusPacket() { }

    public DysonSphereStatusPacket(DysonSphere dysonSphere)
    {
        StarIndex = dysonSphere.starData.index;
        GrossRadius = dysonSphere.grossRadius;
        EnergyReqCurrentTick = dysonSphere.energyReqCurrentTick;
        EnergyGenCurrentTick = dysonSphere.energyGenCurrentTick;
    }

    public int StarIndex { get; set; }
    public float GrossRadius { get; set; }
    public long EnergyReqCurrentTick { get; set; }
    public long EnergyGenCurrentTick { get; set; }
}

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
        if (dysonSphere?.starData == null)
        {
            return;
        }
        StarIndex = dysonSphere.starData.index;
        GrossRadius = dysonSphere.grossRadius;
        EnergyReqCurrentTick = dysonSphere.energyReqCurrentTick;
        EnergyGenCurrentTick = dysonSphere.energyGenCurrentTick;
        EnergyGenOriginalCurrentTick = dysonSphere.energyGenOriginalCurrentTick;
    }

    public int StarIndex { get; set; }
    public float GrossRadius { get; set; }
    public long EnergyReqCurrentTick { get; set; }
    public long EnergyGenCurrentTick { get; set; }
    public long EnergyGenOriginalCurrentTick { get; set; }
}

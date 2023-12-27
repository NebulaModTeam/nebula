namespace NebulaModel.Packets.Factory.Turret;

public class TurretPriorityUpdatePacket
{
    public TurretPriorityUpdatePacket() { }

    public TurretPriorityUpdatePacket(int turretIndex, VSLayerMask vsSettings, int planetId)
    {
        TurretIndex = turretIndex;
        VSSettings = vsSettings;
        PlanetId = planetId;
    }

    public int TurretIndex { get; set; }
    public VSLayerMask VSSettings { get; set; }
    public int PlanetId { get; set; }
}

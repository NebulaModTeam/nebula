namespace NebulaModel.Packets.Factory.Belt;

public class ConnectToMonitorPacket
{
    public ConnectToMonitorPacket() { }

    public ConnectToMonitorPacket(int monitorId, int beltId, int offset, int planetId)
    {
        MonitorId = monitorId;
        BeltId = beltId;
        Offset = offset;
        PlanetId = planetId;
    }

    public int MonitorId { get; }
    public int BeltId { get; }
    public int Offset { get; }
    public int PlanetId { get; }
}

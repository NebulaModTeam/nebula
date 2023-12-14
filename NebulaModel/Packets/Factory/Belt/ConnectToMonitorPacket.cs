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

    public int MonitorId { get; set; }
    public int BeltId { get; set; }
    public int Offset { get; set; }
    public int PlanetId { get; set; }
}

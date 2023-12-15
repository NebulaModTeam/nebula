namespace NebulaModel.Packets.Logistics;

public class ILSRequestShipDock
{
    public ILSRequestShipDock() { }

    public ILSRequestShipDock(int stationGId)
    {
        StationGId = stationGId;
    }

    public int StationGId { get; set; }
}

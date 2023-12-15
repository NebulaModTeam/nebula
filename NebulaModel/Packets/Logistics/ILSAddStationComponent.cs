namespace NebulaModel.Packets.Logistics;

public class ILSAddStationComponent
{
    public ILSAddStationComponent() { }

    public ILSAddStationComponent(int planetId, int stationId, int stationGId, int maxShipCount)
    {
        StationGId = stationGId;
        PlanetId = planetId;
        StationId = stationId;
        MaxShipCount = maxShipCount;
    }

    public int PlanetId { get; set; }
    public int StationId { get; set; }
    public int StationGId { get; set; }

    public int MaxShipCount { get; set; }
}

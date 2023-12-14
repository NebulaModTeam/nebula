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

    public int PlanetId { get; }
    public int StationId { get; }
    public int StationGId { get; }

    public int MaxShipCount { get; }
}

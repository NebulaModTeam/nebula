namespace NebulaModel.Packets.Logistics;

public class ILSRemoveStationComponent
{
    public ILSRemoveStationComponent() { }

    public ILSRemoveStationComponent(int stationId, int planetId, int stationGId)
    {
        StationId = stationId;
        PlanetId = planetId;
        StationGId = stationGId;
    }

    public int StationId { get; }
    public int PlanetId { get; }
    public int StationGId { get; }
}

namespace NebulaModel.Packets.Logistics;

public class StationUIInitialSyncRequest
{
    public StationUIInitialSyncRequest() { }

    public StationUIInitialSyncRequest(int planetId, int stationId, int stationGId)
    {
        PlanetId = planetId;
        StationId = stationId;
        StationGId = stationGId;
    }

    public int PlanetId { get; }
    public int StationId { get; }
    public int StationGId { get; }
}

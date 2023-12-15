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

    public int PlanetId { get; set; }
    public int StationId { get; set; }
    public int StationGId { get; set; }
}

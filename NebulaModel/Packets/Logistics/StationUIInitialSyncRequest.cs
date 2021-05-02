namespace NebulaModel.Packets.Logistics
{
    public class StationUIInitialSyncRequest
    {
        public int PlanetId { get; set; }
        public int StationId { get; set; }
        public int StationGId { get; set; }
        public StationUIInitialSyncRequest() { }
        public StationUIInitialSyncRequest(int planetId, int stationId, int stationGId)
        {
            PlanetId = planetId;
            StationId = stationId;
            StationGId = stationGId;
        }
    }
}

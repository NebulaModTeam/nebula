namespace NebulaModel.Packets.Logistics
{
    public class StationUIInitialSyncRequest
    {
        public int stationGId { get; set; }
        public int planetId { get; set; }
        public StationUIInitialSyncRequest() { }
        public StationUIInitialSyncRequest(int stationGId, int planetId)
        {
            this.stationGId = stationGId;
            this.planetId = planetId;
        }
    }
}

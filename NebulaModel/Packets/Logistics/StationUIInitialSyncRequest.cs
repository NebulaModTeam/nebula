namespace NebulaModel.Packets.Logistics
{
    public class StationUIInitialSyncRequest
    {
        public int stationGId { get; set; }
        public StationUIInitialSyncRequest() { }
        public StationUIInitialSyncRequest(int stationGId)
        {
            this.stationGId = stationGId;
        }
    }
}

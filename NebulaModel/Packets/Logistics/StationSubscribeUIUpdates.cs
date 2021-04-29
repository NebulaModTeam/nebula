namespace NebulaModel.Packets.Logistics
{
    public class StationSubscribeUIUpdates
    {
        public bool subscribe { get; set; }
        public int stationGId { get; set; }
        public StationSubscribeUIUpdates() { }
        public StationSubscribeUIUpdates(bool subscribe, int stationGId)
        {
            this.subscribe = subscribe;
            this.stationGId = stationGId;
        }
    }
}

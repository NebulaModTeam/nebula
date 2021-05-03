namespace NebulaModel.Packets.Logistics
{
    public class StationSubscribeUIUpdates
    {
        public int PlanetId { get; set; }
        public int StationId { get; set; }
        public int StationGId { get; set; }
        public bool Subscribe { get; set; }
        public StationSubscribeUIUpdates() { }
        public StationSubscribeUIUpdates(bool subscribe, int planetId, int stationId, int stationGId)
        {
            PlanetId = planetId;
            StationId = stationId;
            StationGId = stationGId;
            Subscribe = subscribe;
        }
    }
}

namespace NebulaModel.Packets.Logistics
{
    public class ILSRemoveStationComponent
    {
        public int stationId {get; set;}
        public int planetId { get; set; }
        public int stationGId { get; set; }
        public ILSRemoveStationComponent() { }
        public ILSRemoveStationComponent(int stationId, int planetId, int stationGId)
        {
            this.stationId = stationId;
            this.planetId = planetId;
            this.stationGId = stationGId;
        }
    }
}

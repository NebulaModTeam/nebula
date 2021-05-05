namespace NebulaModel.Packets.Logistics
{
    public class ILSRequestShipDock
    {
        public int StationGId { get; set; }
        public ILSRequestShipDock() { }
        public ILSRequestShipDock(int stationGId)
        {
            StationGId = stationGId;
        }
    }
}

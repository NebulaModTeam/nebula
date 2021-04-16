namespace NebulaModel.Packets.Logistics
{
    public class ILSRequestShipDock
    {
        public int stationGId { get; set; }
        public ILSRequestShipDock() { }
        public ILSRequestShipDock(int stationGId)
        {
            this.stationGId = stationGId;
        }
    }
}

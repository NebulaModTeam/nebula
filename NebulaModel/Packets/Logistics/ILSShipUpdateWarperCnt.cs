namespace NebulaModel.Packets.Logistics
{
    public class ILSShipUpdateWarperCnt
    {
        public int stationGId { get; set; }
        public int shipIndex { get; set; }
        public int warperCnt { get; set; }

        public ILSShipUpdateWarperCnt() { }
        public ILSShipUpdateWarperCnt(int stationGId, int shipIndex, int warperCnt)
        {
            this.stationGId = stationGId;
            this.shipIndex = shipIndex;
            this.warperCnt = warperCnt;
        }
    }
}

namespace NebulaModel.Packets.Logistics
{
    public class ILSShipData
    {
        public bool idleToWork { get; set; }
        public int planetA { get; set; }
        public int planetB { get; set; }
        public int itemId { get; set; }
        public int itemCount { get; set; }
        public int planetAStationGID { get; set; }
        
        public int planetAStationMaxShipCount { get; set; }
        public int planetBStationGID { get; set; }
        
        public int planetBStationMaxShipCount { get; set; }
        public int origShipIndex { get; set; }
        public int warperCnt { get; set; }
        public int stationWarperCnt { get; set; }

        public ILSShipData() { }
        public ILSShipData(bool IdleToWork, ShipData ship, int AGID, int aMaxShipCount, int BGID,  int bMaxShipCount, int origShipIndex, int stationWarperCnt)
        {
            idleToWork = IdleToWork;
            planetA = ship.planetA;
            planetB = ship.planetB;
            itemId = ship.itemId;
            itemCount = ship.itemCount;
            warperCnt = ship.warperCnt;
            
            planetAStationGID = AGID;
            planetBStationGID = BGID;
            planetAStationMaxShipCount = aMaxShipCount;
            planetBStationMaxShipCount = bMaxShipCount;
            
            this.origShipIndex = origShipIndex;
            this.stationWarperCnt = stationWarperCnt;
            
        }
        public ILSShipData(bool IdleToWork, int AGID, int origShipIndex)
        {
            idleToWork = IdleToWork;
            planetA = 0;
            planetB = 0;
            itemId = 0;
            itemCount = 0;
            planetAStationGID = AGID;
            planetBStationGID = 0;
            planetAStationMaxShipCount = 10;
            planetBStationMaxShipCount = 10;
            this.origShipIndex = origShipIndex;
            warperCnt = 0;
        }
    }
}

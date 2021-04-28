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
        public int planetBStationGID { get; set; }
        public int origShipIndex { get; set; }
        public int warperCnt { get; set; }
        public int stationWarperCnt { get; set; }

        public ILSShipData() { }
        public ILSShipData(bool IdleToWork, int planetA, int planetB, int itemId, int itemCount, int AGID, int BGID, int origShipIndex, int warperCnt, int stationWarperCnt)
        {
            this.idleToWork = IdleToWork;
            this.planetA = planetA;
            this.planetB = planetB;
            this.itemId = itemId;
            this.itemCount = itemCount;
            this.planetAStationGID = AGID;
            this.planetBStationGID = BGID;
            this.origShipIndex = origShipIndex;
            this.warperCnt = warperCnt;
            this.stationWarperCnt = stationWarperCnt;
        }
        public ILSShipData(bool IdleToWork, int AGID, int origShipIndex)
        {
            this.idleToWork = IdleToWork;
            this.planetA = 0;
            this.planetB = 0;
            this.itemId = 0;
            this.itemCount = 0;
            this.planetAStationGID = AGID;
            this.planetBStationGID = 0;
            this.origShipIndex = origShipIndex;
            this.warperCnt = 0;
        }
    }
}

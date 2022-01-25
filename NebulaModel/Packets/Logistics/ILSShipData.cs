namespace NebulaModel.Packets.Logistics
{
    public class ILSShipData
    {
        public bool IdleToWork { get; set; }
        public int ThisGId { get; set; }
        public int PlanetA { get; set; }
        public int PlanetB { get; set; }
        public int OtherGId { get; set; }
        public int ItemId { get; set; }
        public int ItemCount { get; set; }
        public int Inc { get; set; }
        public int Gene { get; set; }
        public int ShipIndex { get; set; }
        public int ShipWarperCount { get; set; }
        public int StationMaxShipCount { get; set; }
        public int StationWarperCount { get; set; }
        public int WorkShipIndex { get; set; }

        public ILSShipData() { }
        public ILSShipData(bool idleToWork, ShipData ShipData, int thisGId, int stationMaxShipCount, int stationWarperCount)
        {
            IdleToWork = idleToWork;
            ThisGId = thisGId;
            PlanetA = ShipData.planetA;
            PlanetB = ShipData.planetB;
            OtherGId = ShipData.otherGId;
            ItemId = ShipData.itemId;
            ItemCount = ShipData.itemCount;
            Inc = ShipData.inc;
            Gene = ShipData.gene;
            ShipIndex = ShipData.shipIndex;
            ShipWarperCount = ShipData.warperCnt;
            StationMaxShipCount = stationMaxShipCount;
            StationWarperCount = stationWarperCount;
            WorkShipIndex = 0;
            
        }
        public ILSShipData(bool idleToWork, int thisGId, int planetA, int stationMaxShipCount)
        {
            IdleToWork = idleToWork;
            ThisGId = thisGId;
            PlanetA = planetA;
            PlanetB = 0;
            OtherGId = 0;
            ItemId = 0;
            ItemCount = 0;
            Inc = 0;
            Gene = 0;
            ShipIndex = 0;
            StationMaxShipCount = stationMaxShipCount;
            StationWarperCount = 0;
            WorkShipIndex = 0;
        }
    }
}

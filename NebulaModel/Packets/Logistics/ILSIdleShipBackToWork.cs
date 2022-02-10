namespace NebulaModel.Packets.Logistics
{
    public class ILSIdleShipBackToWork
    {
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

        public ILSIdleShipBackToWork() { }
        public ILSIdleShipBackToWork(ShipData ShipData, int thisGId, int stationMaxShipCount, int stationWarperCount)
        {
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
        }
    }
}

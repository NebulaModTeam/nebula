namespace NebulaModel.Packets.Logistics;

public class ILSIdleShipBackToWork
{
    public ILSIdleShipBackToWork() { }

    public ILSIdleShipBackToWork(in ShipData shipData, int thisGId, int stationMaxShipCount, int stationWarperCount)
    {
        ThisGId = thisGId;
        PlanetA = shipData.planetA;
        PlanetB = shipData.planetB;
        OtherGId = shipData.otherGId;
        ItemId = shipData.itemId;
        ItemCount = shipData.itemCount;
        Inc = shipData.inc;
        ShipIndex = shipData.shipIndex;
        ShipWarperCount = (byte)shipData.warperCnt;
        StationMaxShipCount = stationMaxShipCount;
        StationWarperCount = stationWarperCount;
    }

    public int ThisGId { get; set; }
    public int PlanetA { get; set; }
    public int PlanetB { get; set; }
    public int OtherGId { get; set; }
    public int ItemId { get; set; }
    public int ItemCount { get; set; }
    public int Inc { get; set; }
    public int ShipIndex { get; set; }
    public byte ShipWarperCount { get; set; } // Max count for round-trip: 2
    public int StationMaxShipCount { get; set; }
    public int StationWarperCount { get; set; }
}

namespace NebulaModel.Packets.Logistics;

public class ILSIdleShipBackToWork
{
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

    public int ThisGId { get; }
    public int PlanetA { get; }
    public int PlanetB { get; }
    public int OtherGId { get; }
    public int ItemId { get; }
    public int ItemCount { get; }
    public int Inc { get; }
    public int Gene { get; }
    public int ShipIndex { get; }
    public int ShipWarperCount { get; }
    public int StationMaxShipCount { get; }
    public int StationWarperCount { get; }
}

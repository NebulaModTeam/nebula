namespace NebulaModel.Packets.Logistics;

public class ILSWorkShipBackToIdle
{
    public ILSWorkShipBackToIdle() { }

    public ILSWorkShipBackToIdle(StationComponent stationComponent, ShipData shipData, int workShipIndex)
    {
        GId = stationComponent.gid;
        PlanetA = shipData.planetA;
        StationMaxShipCount = stationComponent.workShipDatas.Length;
        ShipIndex = shipData.shipIndex;
        WorkShipIndex = workShipIndex;
    }

    public int GId { get; }
    public int PlanetA { get; }
    public int StationMaxShipCount { get; }
    public int ShipIndex { get; }
    public int WorkShipIndex { get; }
}

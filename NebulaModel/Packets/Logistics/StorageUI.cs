namespace NebulaModel.Packets.Logistics;

public class StorageUI
{
    public StorageUI() { }

    public StorageUI(int planetId, int stationId, int stationGId, int storageIdx, int itemId, int itemCountMax,
        ELogisticStorage localLogic, ELogisticStorage remoteLogic)
    {
        ItemCount = -1; //Indicate it is SetStationStorage()

        PlanetId = planetId;
        StationId = stationId;
        StationGId = stationGId;
        StorageIdx = storageIdx;
        ItemId = itemId;
        ItemCountMax = itemCountMax;
        LocalLogic = localLogic;
        RemoteLogic = remoteLogic;
    }

    public StorageUI(int planetId, int stationId, int stationGId, int storageIdx, int itemCount, int itemInc)
    {
        ShouldRefund = false;

        PlanetId = planetId;
        StationId = stationId;
        StationGId = stationGId;
        StorageIdx = storageIdx;
        ItemCount = itemCount;
        ItemInc = itemInc;
    }

    public StorageUI(int planetId, int stationId, int stationGId, int storageIdx, byte keepMode)
    {
        ItemCount = -2; //Indicate it is other settings update

        PlanetId = planetId;
        StationId = stationId;
        StationGId = stationGId;
        StorageIdx = storageIdx;
        KeepMode = keepMode;
    }

    public int PlanetId { get; }
    public int StationId { get; }
    public int StationGId { get; }
    public int StorageIdx { get; }
    public int ItemId { get; }
    public int ItemCountMax { get; }
    public ELogisticStorage LocalLogic { get; }
    public ELogisticStorage RemoteLogic { get; }
    public int ItemCount { get; }
    public int ItemInc { get; }
    public bool ShouldRefund { get; set; }
    public byte KeepMode { get; }
}

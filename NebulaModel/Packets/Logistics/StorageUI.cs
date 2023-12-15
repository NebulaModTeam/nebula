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

    public int PlanetId { get; set; }
    public int StationId { get; set; }
    public int StationGId { get; set; }
    public int StorageIdx { get; set; }
    public int ItemId { get; set; }
    public int ItemCountMax { get; set; }
    public ELogisticStorage LocalLogic { get; set; }
    public ELogisticStorage RemoteLogic { get; set; }
    public int ItemCount { get; set; }
    public int ItemInc { get; set; }
    public bool ShouldRefund { get; set; }
    public byte KeepMode { get; set; }
}

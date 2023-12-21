namespace NebulaModel.Packets.Factory.Storage;

public class StorageSyncSetFilterPacket
{
    public StorageSyncSetFilterPacket() { }

    // @TODO: This can potentially be moved up the call stack - in UI input - for storage boxes-
    // to avoid sending redundant data like `StorageIndex` & `StorageType` per every grid slot.
    public StorageSyncSetFilterPacket(int storageIndex, int planetId, int gridIndex, int filterId, EStorageType storageType)
    {
        StorageIndex = storageIndex;
        PlanetId = planetId;
        GridIndex = gridIndex;
        FilterId = filterId;
        StorageType = storageType;
    }
    public int StorageIndex { get; set; }
    public int PlanetId { get; set; }
    public int GridIndex { get; set; }
    public int FilterId { get; set; }
    public EStorageType StorageType { get; set; }
}

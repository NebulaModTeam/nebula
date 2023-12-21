namespace NebulaModel.Packets.Factory.Storage;

public class StorageSyncSetFilterPacket
{
    public StorageSyncSetFilterPacket() { }

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

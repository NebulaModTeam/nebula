namespace NebulaModel.Packets.Factory.Storage;

public class StorageSyncSortPacket
{
    public StorageSyncSortPacket() { }

    public StorageSyncSortPacket(int storageIndex, int planetId)
    {
        StorageIndex = storageIndex;
        PlanetId = planetId;
    }

    public int StorageIndex { get; set; }
    public int PlanetId { get; set; }
}

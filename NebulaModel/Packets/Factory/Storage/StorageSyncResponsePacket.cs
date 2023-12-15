namespace NebulaModel.Packets.Factory.Storage;

public class StorageSyncResponsePacket
{
    public StorageSyncResponsePacket() { }

    public StorageSyncResponsePacket(int planetId, int storageIndex, byte[] storageComponent)
    {
        StorageIndex = storageIndex;
        StorageComponent = storageComponent;
        PlanetId = planetId;
    }

    public byte[] StorageComponent { get; set; }
    public int PlanetId { get; set; }
    public int StorageIndex { get; set; }
}

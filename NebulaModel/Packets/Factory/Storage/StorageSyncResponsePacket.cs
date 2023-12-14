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

    public byte[] StorageComponent { get; }
    public int PlanetId { get; }
    public int StorageIndex { get; }
}

namespace NebulaModel.Packets.Factory.Storage;

public class StorageSyncSetBansPacket
{
    public StorageSyncSetBansPacket() { }

    public StorageSyncSetBansPacket(int storageIndex, int planetId, int bans)
    {
        StorageIndex = storageIndex;
        PlanetId = planetId;
        Bans = bans;
    }

    public int StorageIndex { get; }
    public int PlanetId { get; }
    public int Bans { get; }
}

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

    public int StorageIndex { get; set; }
    public int PlanetId { get; set; }
    public int Bans { get; set; }
}

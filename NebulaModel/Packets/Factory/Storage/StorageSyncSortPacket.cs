namespace NebulaModel.Packets.Factory.Storage
{
    public class StorageSyncSortPacket
    {
        public int StorageIndex { get; set; }
        public int PlanetId { get; set; }

        public StorageSyncSortPacket() { }

        public StorageSyncSortPacket(int storageIndex, int planetId)
        {
            StorageIndex = storageIndex;
            PlanetId = planetId;
        }
    }
}

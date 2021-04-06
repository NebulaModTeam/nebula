namespace NebulaModel.Packets.Factory
{
    public class StorageSyncSortPacket
    {
        public int StorageIndex { get; set; }
        public int FactoryIndex { get; set; }

        public StorageSyncSortPacket() { }

        public StorageSyncSortPacket(int storageIndex, int factoryIndex)
        {
            StorageIndex = storageIndex;
            FactoryIndex = factoryIndex;
        }
    }
}

namespace NebulaModel.Packets.Factory
{
    public class StorageSyncResponsePacket
    {
        public byte[] StorageComponent { get; set; }
        public int FactoryIndex { get; set; }
        public int StorageIndex { get; set; }

        public StorageSyncResponsePacket() { }

        public StorageSyncResponsePacket(int factoryIndex, int storageIndex, byte[] storageComponent)
        {
            StorageIndex = storageIndex;
            StorageComponent = storageComponent;
            FactoryIndex = factoryIndex;
        }
    }
}

namespace NebulaModel.Packets.Factory
{
    public class StorageSyncSetBansPacket
    {
        public int StorageIndex { get; set; }
        public int FactoryIndex { get; set; }
        public int Bans { get; set; }

        public StorageSyncSetBansPacket() { }

        public StorageSyncSetBansPacket(int storageIndex, int factoryIndex, int bans)
        {
            StorageIndex = storageIndex;
            FactoryIndex = factoryIndex;
            Bans = bans;
        }
    }
}

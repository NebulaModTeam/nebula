namespace NebulaModel.Packets.Factory.Storage
{
    public class StorageSyncRequestPacket
    {
        public int PlanetId { get; set; }
        public int StorageId { get; set; }

        public StorageSyncRequestPacket() { }

        public StorageSyncRequestPacket(int planetId, int storageId)
        {
            PlanetId = planetId;
            StorageId = storageId;
        }
    }
}

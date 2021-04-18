using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaHost.PacketProcessors.Factory.Storage
{
    [RegisterPacketProcessor]
    class StorageSyncSortProcessor : IPacketProcessor<StorageSyncSortPacket>
    {
        public void ProcessPacket(StorageSyncSortPacket packet, NebulaConnection conn)
        {
            StorageComponent storage = null;
            StorageComponent[] pool = GameMain.data.factories[packet.FactoryIndex]?.factoryStorage?.storagePool;
            if (pool != null && packet.StorageIndex != -1 && packet.StorageIndex < pool.Length)
            {
                storage = pool[packet.StorageIndex];
            }

            if (storage != null)
            {
                using (StorageManager.EventFromClient.On())
                {
                    storage.Sort();
                }
            }
        }
    }
}
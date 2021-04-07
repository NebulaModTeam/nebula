using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaHost.PacketProcessors.Factory
{
    [RegisterPacketProcessor]
    class StorageSyncSortProcessor : IPacketProcessor<StorageSyncSortPacket>
    {
        public void ProcessPacket(StorageSyncSortPacket packet, NebulaConnection conn)
        {
            StorageComponent storage = null;
            StorageComponent[] pool = GameMain.localPlanet?.factory?.factoryStorage?.storagePool;
            if (pool != null && packet.StorageIndex != -1 && packet.StorageIndex < pool.Length)
            {
                storage = pool[packet.StorageIndex];
            }

            if (storage != null)
            {
                StorageManager.EventFromClient = true;
                storage.Sort();
                StorageManager.EventFromClient = false;
            }
        }
    }
}
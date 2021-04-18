using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaClient.PacketProcessors.Factory.Storage
{
    [RegisterPacketProcessor]
    class StorageSyncSetBansProcessor : IPacketProcessor<StorageSyncSetBansPacket>
    {
        public void ProcessPacket(StorageSyncSetBansPacket packet, NebulaConnection conn)
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
                    storage.SetBans(packet.Bans);
                }
            }
        }
    }
}
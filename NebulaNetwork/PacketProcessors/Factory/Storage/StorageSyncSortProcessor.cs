using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;
using NebulaWorld.Factory;

namespace NebulaNetwork.PacketProcessors.Factory.Storage
{
    [RegisterPacketProcessor]
    class StorageSyncSortProcessor : PacketProcessor<StorageSyncSortPacket>
    {
        public override void ProcessPacket(StorageSyncSortPacket packet, NebulaConnection conn)
        {
            StorageComponent storage = null;
            StorageComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factoryStorage?.storagePool;
            if (pool != null && packet.StorageIndex != -1 && packet.StorageIndex < pool.Length)
            {
                storage = pool[packet.StorageIndex];
            }

            if (storage != null)
            {
                using (StorageManager.IsIncomingRequest.On())
                {
                    storage.Sort();
                }
            }
        }
    }
}
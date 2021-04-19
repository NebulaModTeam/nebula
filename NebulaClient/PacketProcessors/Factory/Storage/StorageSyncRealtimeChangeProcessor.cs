using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaClient.PacketProcessors.Factory.Storage
{
    [RegisterPacketProcessor]
    class StorageSyncRealtimeChangeProcessor : IPacketProcessor<StorageSyncRealtimeChangePacket>
    {
        public void ProcessPacket(StorageSyncRealtimeChangePacket packet, NebulaConnection conn)
        {
            StorageComponent storageComponent = null;
            StorageComponent[] pool = GameMain.data.factories[packet.FactoryIndex]?.factoryStorage?.storagePool;
            if (pool != null && packet.StorageIndex != -1 && packet.StorageIndex < pool.Length)
            {
                storageComponent = pool[packet.StorageIndex];
            }

            if (storageComponent != null)
            {
                using (StorageManager.EventFromServer.On())
                {
                    int itemId = packet.ItemId;
                    int count = packet.Count;
                    if (packet.StorageEvent == StorageSyncRealtimeChangeEvent.AddItem2)
                    {
                        storageComponent.AddItem(packet.ItemId, packet.Count, packet.StartIndex, packet.Length);
                    }
                    else if (packet.StorageEvent == StorageSyncRealtimeChangeEvent.AddItemStacked)
                    {
                        storageComponent.AddItemStacked(packet.ItemId, packet.Count);
                    }
                    else if (packet.StorageEvent == StorageSyncRealtimeChangeEvent.TakeItemFromGrid)
                    {
                        storageComponent.TakeItemFromGrid(packet.Length, ref itemId, ref count);
                    }
                }
            }
        }
    }
}
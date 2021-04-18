using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaHost.PacketProcessors.Factory.Storage
{
    [RegisterPacketProcessor]
    class StorageSyncRealtimeChangeProcessor : IPacketProcessor<StorageSyncRealtimeChangePacket>
    {
        public void ProcessPacket(StorageSyncRealtimeChangePacket packet, NebulaConnection conn)
        {
            int itemId = packet.ItemId;
            int count = packet.Count;

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
                    if (packet.StorageEvent == StorageSyncRealtimeChangeEvent.TakeItemFromGrid)
                    {
                        storage.TakeItemFromGrid(packet.Length, ref itemId, ref count);
                        StorageSyncManager.SendToOtherPlayersOnTheSamePlanet(conn, packet, packet.PlanetId);
                    }
                    else if (packet.StorageEvent == StorageSyncRealtimeChangeEvent.AddItem2)
                    {
                        storage.AddItem(itemId, count, packet.StartIndex, packet.Length);
                        StorageSyncManager.SendToOtherPlayersOnTheSamePlanet(conn, packet, packet.PlanetId);
                    }
                    else if (packet.StorageEvent == StorageSyncRealtimeChangeEvent.AddItemStacked)
                    {
                        int result = storage.AddItemStacked(itemId, count);
                        StorageSyncManager.SendToOtherPlayersOnTheSamePlanet(conn, packet, packet.PlanetId);
                    }
                }
            }
        }
    }
}
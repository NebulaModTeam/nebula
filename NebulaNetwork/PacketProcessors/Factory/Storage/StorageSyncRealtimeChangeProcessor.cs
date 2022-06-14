using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Storage;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Factory.Storage
{
    [RegisterPacketProcessor]
    internal class StorageSyncRealtimeChangeProcessor : PacketProcessor<StorageSyncRealtimeChangePacket>
    {
        public override void ProcessPacket(StorageSyncRealtimeChangePacket packet, NebulaConnection conn)
        {
            StorageComponent storage = null;
            StorageComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factoryStorage?.storagePool;
            if (pool != null && packet.StorageIndex != -1 && packet.StorageIndex < pool.Length)
            {
                storage = pool[packet.StorageIndex];
            }

            if (storage != null)
            {
                using (Multiplayer.Session.Storage.IsIncomingRequest.On())
                {
                    int itemId = packet.ItemId;
                    int count = packet.Count;
                    int inc = packet.Inc;
                    int dummyOut;
                    if (packet.StorageEvent == StorageSyncRealtimeChangeEvent.AddItem2)
                    {
                        storage.AddItem(itemId, count, packet.StartIndex, packet.Length, inc, out dummyOut);
                    }
                    else if (packet.StorageEvent == StorageSyncRealtimeChangeEvent.AddItemStacked)
                    {
                        storage.AddItemStacked(itemId, count, inc, out dummyOut);
                    }
                    else if (packet.StorageEvent == StorageSyncRealtimeChangeEvent.TakeItemFromGrid)
                    {
                        storage.TakeItemFromGrid(packet.Length, ref itemId, ref count, out dummyOut);
                    }

                    if (IsHost)
                    {
                        int starId = GameMain.galaxy.PlanetById(packet.PlanetId).star.id;
                        Multiplayer.Session.Network.SendPacketToStarExclude(packet, starId, conn);
                    }
                }
            }
        }
    }
}
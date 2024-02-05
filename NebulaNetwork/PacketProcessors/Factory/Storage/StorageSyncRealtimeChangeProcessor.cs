#region

using System;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Storage;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Storage;

[RegisterPacketProcessor]
internal class StorageSyncRealtimeChangeProcessor : PacketProcessor<StorageSyncRealtimeChangePacket>
{
    protected override void ProcessPacket(StorageSyncRealtimeChangePacket packet, NebulaConnection conn)
    {
        StorageComponent storage = null;
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factoryStorage?.storagePool;
        if (pool != null && packet.StorageIndex != -1 && packet.StorageIndex < pool.Length)
        {
            storage = pool[packet.StorageIndex];
        }

        if (storage == null)
        {
            return;
        }
        using (Multiplayer.Session.Storage.IsIncomingRequest.On())
        {
            var itemId = packet.ItemId;
            var count = packet.Count;
            var inc = packet.Inc;
            switch (packet.StorageEvent)
            {
                case StorageSyncRealtimeChangeEvent.AddItem1:
                    storage.AddItem(itemId, count, inc, out _, true);
                    break;
                case StorageSyncRealtimeChangeEvent.AddItem2:
                    storage.AddItem(itemId, count, packet.StartIndex, packet.Length, inc, out _);
                    break;
                case StorageSyncRealtimeChangeEvent.AddItemStacked:
                    storage.AddItemStacked(itemId, count, inc, out _);
                    break;
                case StorageSyncRealtimeChangeEvent.AddItemFiltered: // Use by BattleBaseComponent.AutoPickTrash
                    storage.AddItemFiltered(itemId, count, inc, out _, true);
                    break;
                case StorageSyncRealtimeChangeEvent.TakeItem:
                    break;
                case StorageSyncRealtimeChangeEvent.TakeItemFromGrid:
                    storage.TakeItemFromGrid(packet.Length, ref itemId, ref count, out _);
                    break;
                case StorageSyncRealtimeChangeEvent.TakeHeadItems:
                    break;
                case StorageSyncRealtimeChangeEvent.TakeTailItems1:
                    break;
                case StorageSyncRealtimeChangeEvent.TakeTailItems2:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(packet), "Unknown StorageSyncRealtimeChangeEvent type: " + packet.StorageEvent);
            }

            if (!IsHost)
            {
                return;
            }
            var starId = GameMain.galaxy.PlanetById(packet.PlanetId).star.id;
            Multiplayer.Session.Network.SendPacketToStarExclude(packet, starId, conn);
        }
    }
}

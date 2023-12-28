#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Storage;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Storage;

[RegisterPacketProcessor]
public class StorageSyncSetFilterProcessor : PacketProcessor<StorageFilterSyncPacket>
{
    protected override void ProcessPacket(StorageFilterSyncPacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factoryStorage?.storagePool;

        var poolIsInvalid = pool is null;
        if (poolIsInvalid)
            return;

        using (Multiplayer.Session.Storage.IsIncomingRequest.On())
            foreach (var kvp in packet.StorageFilters)
            {
                var storageIndex = kvp.Key;
                var packetData = kvp.Value;
                StorageComponent storage;

                var storageIsValid = storageIndex != -1 && storageIndex < pool.Length;
                if (storageIsValid)
                    storage = pool[storageIndex];
                else
                    continue;

                storage.type = packetData.StorageType;
                foreach (var kvFilter in packetData.GridFilters)
                {
                    storage.SetFilter(kvFilter.Key, kvFilter.Value);
                }

                storage.NotifyStorageChange();
            }
    }
}

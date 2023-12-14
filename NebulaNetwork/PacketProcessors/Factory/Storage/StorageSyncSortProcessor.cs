#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Storage;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Storage;

[RegisterPacketProcessor]
internal class StorageSyncSortProcessor : PacketProcessor<StorageSyncSortPacket>
{
    protected override void ProcessPacket(StorageSyncSortPacket packet, NebulaConnection conn)
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
            storage.Sort();
        }
    }
}

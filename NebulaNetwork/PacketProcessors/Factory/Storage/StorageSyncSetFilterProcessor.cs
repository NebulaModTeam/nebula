#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Storage;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Storage;

[RegisterPacketProcessor]
public class StorageSyncSetFilterProcessor : PacketProcessor<StorageSyncSetFilterPacket>
{
    protected override void ProcessPacket(StorageSyncSetFilterPacket packet, NebulaConnection conn)
    {
        StorageComponent storage = null;
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factoryStorage?.storagePool;

        var poolIsInvalid = pool is null;
        if (poolIsInvalid)
        {
            return;
        }

        var storageIsValid = packet.StorageIndex != -1 && packet.StorageIndex < pool.Length;
        if (storageIsValid)
        {
            storage = pool[packet.StorageIndex];
        }

        if (storage is null)
            return;

        using (Multiplayer.Session.Storage.IsIncomingRequest.On())
        {
            storage.type = packet.StorageType;
            storage.SetFilter(packet.GridIndex, packet.FilterId);
            storage.NotifyStorageChange();
        }
    }
}

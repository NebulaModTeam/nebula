#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Storage;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Storage;

[RegisterPacketProcessor]
internal class StorageSyncRequestProcessor : PacketProcessor<StorageSyncRequestPacket>
{
    protected override void ProcessPacket(StorageSyncRequestPacket packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            return;
        }

        if (GameMain.galaxy.PlanetById(packet.PlanetId) == null)
        {
            return;
        }
        var storageComponent = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factoryStorage
            ?.storagePool[packet.StorageId];
        if (storageComponent == null)
        {
            return;
        }
        using var writer = new BinaryUtils.Writer();
        storageComponent.Export(writer.BinaryWriter);
        conn.SendPacket(new StorageSyncResponsePacket(packet.PlanetId, packet.StorageId,
            writer.CloseAndGetBytes()));
    }
}

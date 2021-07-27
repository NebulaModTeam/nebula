using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;

namespace NebulaNetwork.PacketProcessors.Factory.Storage
{
    [RegisterPacketProcessor]
    class StorageSyncRequestProcessor : PacketProcessor<StorageSyncRequestPacket>
    {
        public override void ProcessPacket(StorageSyncRequestPacket packet, NebulaConnection conn)
        {
            if (IsClient) return;

            if (GameMain.galaxy.PlanetById(packet.PlanetId) != null)
            {
                StorageComponent storageComponent = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factoryStorage?.storagePool[packet.StorageId];
                if (storageComponent != null)
                {
                    using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
                    {
                        storageComponent.Export(writer.BinaryWriter);
                        conn.SendPacket(new StorageSyncResponsePacket(packet.PlanetId, packet.StorageId, writer.CloseAndGetBytes()));
                    }
                }
            }
        }
    }
}

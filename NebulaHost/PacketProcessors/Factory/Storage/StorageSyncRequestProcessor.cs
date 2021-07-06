using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;

namespace NebulaHost.PacketProcessors.Factory.Storage
{
    [RegisterPacketProcessor]
    class StorageSyncRequestProcessor : IPacketProcessor<StorageSyncRequestPacket>
    {
        public void ProcessPacket(StorageSyncRequestPacket packet, NebulaConnection conn)
        {
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

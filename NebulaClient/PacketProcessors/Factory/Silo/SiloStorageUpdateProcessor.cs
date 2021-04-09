using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.Silo;
using NebulaModel.Packets.Processors;

namespace NebulaClient.PacketProcessors.Factory.Silo
{
    [RegisterPacketProcessor]
    class SiloStorageUpdateProcessor : IPacketProcessor<SiloStorageUpdatePacket>
    {
        public void ProcessPacket(SiloStorageUpdatePacket packet, NebulaConnection conn)
        {
            SiloComponent[] pool = GameMain.data.factories[packet.FactoryIndex]?.factorySystem?.siloPool;
            if (pool != null && packet.SiloIndex != -1 && packet.SiloIndex < pool.Length && pool[packet.SiloIndex].id != -1)
            {
                pool[packet.SiloIndex].bulletCount = packet.NewRocketsAmount;
            }
        }
    }
}
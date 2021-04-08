using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Attributes;
using NebulaModel.Packets.Factory.Silo;

namespace NebulaClient.PacketProcessors.Factory.Silo
{
    [RegisterPacketProcessor]
    class SiloStorageUpdateProcessor : IPacketProcessor<SiloStorageUpdatePacket>
    {
        public void ProcessPacket(SiloStorageUpdatePacket packet, NebulaConnection conn)
        {
            SiloComponent[] pool = GameMain.localPlanet?.factory?.factorySystem.siloPool;
            if (pool != null && packet.SiloIndex != -1 && packet.SiloIndex < pool.Length && pool[packet.SiloIndex].id != -1)
            {
                pool[packet.SiloIndex].bulletCount = packet.NewRocketsAmount;
            }
        }
    }
}
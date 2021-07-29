using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Silo;

namespace NebulaNetwork.PacketProcessors.Factory.Silo
{
    [RegisterPacketProcessor]
    class SiloStorageUpdateProcessor : PacketProcessor<SiloStorageUpdatePacket>
    {
        public override void ProcessPacket(SiloStorageUpdatePacket packet, NebulaConnection conn)
        {
            SiloComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factorySystem?.siloPool;
            if (pool != null && packet.SiloIndex != -1 && packet.SiloIndex < pool.Length && pool[packet.SiloIndex].id != -1)
            {
                pool[packet.SiloIndex].bulletCount = packet.NewRocketsAmount;
            }
        }
    }
}
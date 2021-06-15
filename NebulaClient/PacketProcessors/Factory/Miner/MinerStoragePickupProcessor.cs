using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.Miner;
using NebulaModel.Packets.Processors;

namespace NebulaClient.PacketProcessors.Factory.Miner
{
    [RegisterPacketProcessor]
    class MinerStoragePickupProcessor : IPacketProcessor<MinerStoragePickupPacket>
    {
        public void ProcessPacket(MinerStoragePickupPacket packet, NebulaConnection conn)
        {
            MinerComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factorySystem?.minerPool;
            if (pool != null && packet.MinerIndex != -1 && packet.MinerIndex < pool.Length && pool[packet.MinerIndex].id != -1)
            {
                pool[packet.MinerIndex].productCount = 0;
                pool[packet.MinerIndex].productId = 0;
            }
        }
    }
}
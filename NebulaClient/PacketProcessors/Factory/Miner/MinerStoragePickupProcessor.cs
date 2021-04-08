using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Attributes;
using NebulaModel.Packets.Factory.Miner;

namespace NebulaClient.PacketProcessors.Factory.Miner
{
    [RegisterPacketProcessor]
    class MinerStoragePickupProcessor : IPacketProcessor<MinerStoragePickupPacket>
    {
        public void ProcessPacket(MinerStoragePickupPacket packet, NebulaConnection conn)
        {
            MinerComponent[] pool = GameMain.localPlanet?.factory?.factorySystem?.minerPool;
            if (pool != null && packet.MinerIndex != -1 && packet.MinerIndex < pool.Length && pool[packet.MinerIndex].id != -1)
            {
                pool[packet.MinerIndex].productCount = 0;
                pool[packet.MinerIndex].productId = 0;
            }
        }
    }
}
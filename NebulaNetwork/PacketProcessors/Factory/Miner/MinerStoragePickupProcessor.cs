using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Miner;

namespace NebulaNetwork.PacketProcessors.Factory.Miner
{
    [RegisterPacketProcessor]
    class MinerStoragePickupProcessor : PacketProcessor<MinerStoragePickupPacket>
    {
        public override void ProcessPacket(MinerStoragePickupPacket packet, NebulaConnection conn)
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
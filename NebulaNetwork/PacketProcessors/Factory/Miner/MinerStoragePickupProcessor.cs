#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Miner;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Miner;

[RegisterPacketProcessor]
internal class MinerStoragePickupProcessor : PacketProcessor<MinerStoragePickupPacket>
{
    protected override void ProcessPacket(MinerStoragePickupPacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factorySystem?.minerPool;
        if (pool == null || packet.MinerIndex == -1 || packet.MinerIndex >= pool.Length || pool[packet.MinerIndex].id == -1)
        {
            return;
        }
        pool[packet.MinerIndex].productCount = 0;
        pool[packet.MinerIndex].productId = 0;
    }
}

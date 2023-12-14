#region

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.PowerExchanger;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.PowerExchanger;

[RegisterPacketProcessor]
internal class PowerExchangerStorageUpdateProcessor : PacketProcessor<PowerExchangerStorageUpdatePacket>
{
    public override void ProcessPacket(PowerExchangerStorageUpdatePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.powerSystem?.excPool;
        if (pool != null && packet.PowerExchangerIndex != -1 && packet.PowerExchangerIndex < pool.Length &&
            pool[packet.PowerExchangerIndex].id != -1)
        {
            pool[packet.PowerExchangerIndex].SetEmptyCount(packet.EmptyAccumulatorCount);
            pool[packet.PowerExchangerIndex].SetFullCount(packet.FullAccumulatorCount);
        }
    }
}

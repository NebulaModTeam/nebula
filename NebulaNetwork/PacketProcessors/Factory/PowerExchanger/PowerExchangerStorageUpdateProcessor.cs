using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.PowerExchanger;

namespace NebulaNetwork.PacketProcessors.Factory.PowerExchanger
{
    [RegisterPacketProcessor]
    class PowerExchangerStorageUpdateProcessor : PacketProcessor<PowerExchangerStorageUpdatePacket>
    {
        public override void ProcessPacket(PowerExchangerStorageUpdatePacket packet, NebulaConnection conn)
        {
            PowerExchangerComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.powerSystem?.excPool;
            if (pool != null && packet.PowerExchangerIndex != -1 && packet.PowerExchangerIndex < pool.Length && pool[packet.PowerExchangerIndex].id != -1)
            {
                pool[packet.PowerExchangerIndex].SetEmptyCount(packet.EmptyAccumulatorCount);
                pool[packet.PowerExchangerIndex].SetFullCount(packet.FullAccumulatorCount);
            }
        }
    }
}
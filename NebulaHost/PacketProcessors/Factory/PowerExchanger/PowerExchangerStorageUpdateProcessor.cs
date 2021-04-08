using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.PowerExchanger;
using NebulaModel.Packets.Processors;

namespace NebulaHost.PacketProcessors.Factory.PowerExchanger
{
    [RegisterPacketProcessor]
    class PowerExchangerStorageUpdateProcessor : IPacketProcessor<PowerExchangerStorageUpdatePacket>
    {
        public void ProcessPacket(PowerExchangerStorageUpdatePacket packet, NebulaConnection conn)
        {
            PowerExchangerComponent[] pool = GameMain.localPlanet?.factory?.powerSystem?.excPool;
            if (pool != null && packet.PowerExchangerIndex != -1 && packet.PowerExchangerIndex < pool.Length && pool[packet.PowerExchangerIndex].id != -1)
            {
                pool[packet.PowerExchangerIndex].SetEmptyCount(packet.EmptyAccumulatorCount);
                pool[packet.PowerExchangerIndex].SetFullCount(packet.FullAccumulatorCount);
            }
        }
    }
}
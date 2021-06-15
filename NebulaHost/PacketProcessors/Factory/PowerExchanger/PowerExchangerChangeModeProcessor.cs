using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.PowerExchanger;
using NebulaModel.Packets.Processors;

namespace NebulaHost.PacketProcessors.Factory.PowerExchanger
{
    [RegisterPacketProcessor]
    class PowerExchangerChangeModeProcessor : IPacketProcessor<PowerExchangerChangeModePacket>
    {
        public void ProcessPacket(PowerExchangerChangeModePacket packet, NebulaConnection conn)
        {
            PowerExchangerComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.powerSystem?.excPool;
            if (pool != null && packet.PowerExchangerIndex != -1 && packet.PowerExchangerIndex < pool.Length && pool[packet.PowerExchangerIndex].id != -1)
            {
                pool[packet.PowerExchangerIndex].targetState = packet.Mode;
            }
        }
    }
}
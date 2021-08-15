using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.PowerExchanger;

namespace NebulaNetwork.PacketProcessors.Factory.PowerExchanger
{
    [RegisterPacketProcessor]
    class PowerExchangerChangeModeProcessor : PacketProcessor<PowerExchangerChangeModePacket>
    {
        public override void ProcessPacket(PowerExchangerChangeModePacket packet, NebulaConnection conn)
        {
            PowerExchangerComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.powerSystem?.excPool;
            if (pool != null && packet.PowerExchangerIndex != -1 && packet.PowerExchangerIndex < pool.Length && pool[packet.PowerExchangerIndex].id != -1)
            {
                pool[packet.PowerExchangerIndex].targetState = packet.Mode;
            }
        }
    }
}
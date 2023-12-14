#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.PowerExchanger;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.PowerExchanger;

[RegisterPacketProcessor]
internal class PowerExchangerChangeModeProcessor : PacketProcessor<PowerExchangerChangeModePacket>
{
    protected override void ProcessPacket(PowerExchangerChangeModePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.powerSystem?.excPool;
        if (pool != null && packet.PowerExchangerIndex != -1 && packet.PowerExchangerIndex < pool.Length &&
            pool[packet.PowerExchangerIndex].id != -1)
        {
            pool[packet.PowerExchangerIndex].targetState = packet.Mode;
        }
    }
}

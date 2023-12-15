#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Assembler;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Assembler;

[RegisterPacketProcessor]
internal class AssemblerUpdateProducesProcessor : PacketProcessor<AssemblerUpdateProducesPacket>
{
    protected override void ProcessPacket(AssemblerUpdateProducesPacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factorySystem?.assemblerPool;
        if (pool != null && packet.AssemblerIndex != -1 && packet.AssemblerIndex < pool.Length &&
            pool[packet.AssemblerIndex].id != -1)
        {
            pool[packet.AssemblerIndex].produced[packet.ProducesIndex] = packet.ProducesValue;
        }
    }
}

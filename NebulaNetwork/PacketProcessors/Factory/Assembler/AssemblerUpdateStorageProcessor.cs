#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Assembler;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Assembler;

[RegisterPacketProcessor]
internal class AssemblerUpdateStorageProcessor : PacketProcessor<AssemblerUpdateStoragePacket>
{
    protected override void ProcessPacket(AssemblerUpdateStoragePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factorySystem?.assemblerPool;
        if (pool == null || packet.AssemblerIndex == -1 || packet.AssemblerIndex >= pool.Length ||
            pool[packet.AssemblerIndex].id == -1)
        {
            return;
        }
        for (var i = 0; i < packet.Served.Length; i++)
        {
            pool[packet.AssemblerIndex].served[i] = packet.Served[i];
            pool[packet.AssemblerIndex].incServed[i] = packet.IncServed[i];
        }
    }
}

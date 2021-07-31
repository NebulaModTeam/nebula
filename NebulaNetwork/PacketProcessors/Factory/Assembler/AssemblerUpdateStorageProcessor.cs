using Mirror;
using NebulaModel.Attributes;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Assembler;

namespace NebulaNetwork.PacketProcessors.Factory.Assembler
{
    [RegisterPacketProcessor]
    class AssemblerUpdateStorageProcessor : PacketProcessor<AssemblerUpdateStoragePacket>
    {
        public override void ProcessPacket(AssemblerUpdateStoragePacket packet, NetworkConnection conn)
        {
            AssemblerComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factorySystem?.assemblerPool;
            if (pool != null && packet.AssemblerIndex != -1 && packet.AssemblerIndex < pool.Length && pool[packet.AssemblerIndex].id != -1)
            {
                for (int i = 0; i < packet.Served.Length; i++)
                {
                    pool[packet.AssemblerIndex].served[i] = packet.Served[i];
                }
            }
        }
    }
}
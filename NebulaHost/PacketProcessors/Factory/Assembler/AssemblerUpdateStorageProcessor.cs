using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.Assembler;
using NebulaModel.Packets.Processors;

namespace NebulaHost.PacketProcessors.Factory.Assembler
{
    [RegisterPacketProcessor]
    class AssemblerUpdateStorageProcessor : IPacketProcessor<AssemblerUpdateStoragePacket>
    {
        public void ProcessPacket(AssemblerUpdateStoragePacket packet, NebulaConnection conn)
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
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.Assembler;
using NebulaModel.Packets.Processors;

namespace NebulaHost.PacketProcessors.Factory.Assembler
{
    [RegisterPacketProcessor]
    class AssemblerUpdateProducesProcessor : IPacketProcessor<AssemblerUpdateProducesPacket>
    {
        public void ProcessPacket(AssemblerUpdateProducesPacket packet, NebulaConnection conn)
        {
            AssemblerComponent[] pool = GameMain.data.factories[packet.FactoryIndex]?.factorySystem?.assemblerPool;
            if (pool != null && packet.AssemblerIndex != -1 && packet.AssemblerIndex < pool.Length && pool[packet.AssemblerIndex].id != -1)
            {
                pool[packet.AssemblerIndex].produced[packet.ProducesIndex] = packet.ProducesValue;
            }
        }
    }
}
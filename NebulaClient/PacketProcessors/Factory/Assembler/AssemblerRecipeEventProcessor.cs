using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.Assembler;
using NebulaModel.Packets.Processors;

namespace NebulaClient.PacketProcessors.Factory.Assembler
{
    [RegisterPacketProcessor]
    class AssemblerRecipeEventProcessor : IPacketProcessor<AssemblerRecipeEventPacket>
    {
        public void ProcessPacket(AssemblerRecipeEventPacket packet, NebulaConnection conn)
        {
            AssemblerComponent[] pool = GameMain.data.factories[packet.FactoryIndex]?.factorySystem?.assemblerPool;
            if (pool != null && packet.AssemblerIndex != -1 && packet.AssemblerIndex < pool.Length && pool[packet.AssemblerIndex].id != -1)
            {
                pool[packet.AssemblerIndex].SetRecipe(packet.RecipeId, GameMain.data.factories[packet.FactoryIndex].entitySignPool);
            }
        }
    }
}
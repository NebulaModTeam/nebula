using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Attributes;
using NebulaModel.Packets.Factory.Assembler;

namespace NebulaHost.PacketProcessors.Factory.Assembler
{
    [RegisterPacketProcessor]
    class AssemblerRecipeEventProcessor : IPacketProcessor<AssemblerRecipeEventPacket>
    {
        public void ProcessPacket(AssemblerRecipeEventPacket packet, NebulaConnection conn)
        {
            AssemblerComponent[] pool = GameMain.localPlanet?.factory?.factorySystem.assemblerPool;
            if (pool != null && packet.AssemblerIndex != -1 && packet.AssemblerIndex < pool.Length && pool[packet.AssemblerIndex].id != -1)
            {
                pool[packet.AssemblerIndex].SetRecipe(packet.RecipeId, GameMain.localPlanet.factory.entitySignPool);
            }
        }
    }
}
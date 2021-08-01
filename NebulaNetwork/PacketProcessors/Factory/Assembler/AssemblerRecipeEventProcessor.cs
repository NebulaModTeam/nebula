using Mirror;
using NebulaModel.Attributes;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Assembler;

namespace NebulaNetwork.PacketProcessors.Factory.Assembler
{
    [RegisterPacketProcessor]
    class AssemblerRecipeEventProcessor : PacketProcessor<AssemblerRecipeEventPacket>
    {
        public override void ProcessPacket(AssemblerRecipeEventPacket packet, NetworkConnection conn)
        {
            AssemblerComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factorySystem?.assemblerPool;
            if (pool != null && packet.AssemblerIndex != -1 && packet.AssemblerIndex < pool.Length && pool[packet.AssemblerIndex].id != -1)
            {
                pool[packet.AssemblerIndex].SetRecipe(packet.RecipeId, GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.entitySignPool);
            }
        }
    }
}
#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Assembler;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Assembler;

[RegisterPacketProcessor]
internal class AssemblerRecipeEventProcessor : PacketProcessor<AssemblerRecipeEventPacket>
{
    protected override void ProcessPacket(AssemblerRecipeEventPacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factorySystem?.assemblerPool;
        if (pool == null || packet.AssemblerIndex == -1 || packet.AssemblerIndex >= pool.Length ||
            pool[packet.AssemblerIndex].id == -1)
        {
            return;
        }
        if (packet.RecipeId == -1)
        {
            pool[packet.AssemblerIndex].forceAccMode = !pool[packet.AssemblerIndex].forceAccMode;
        }
        else
        {
            pool[packet.AssemblerIndex].SetRecipe(packet.RecipeId,
                GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.entitySignPool);
        }
    }
}

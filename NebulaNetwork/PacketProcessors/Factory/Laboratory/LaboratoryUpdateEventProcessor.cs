using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Laboratory;

namespace NebulaNetwork.PacketProcessors.Factory.Labratory
{
    [RegisterPacketProcessor]
    class LaboratoryUpdateEventProcessor : PacketProcessor<LaboratoryUpdateEventPacket>
    {
        public override void ProcessPacket(LaboratoryUpdateEventPacket packet, NebulaConnection conn)
        {
            LabComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factorySystem?.labPool;
            if (pool != null && packet.LabIndex != -1 && packet.LabIndex < pool.Length && pool[packet.LabIndex].id != -1)
            {
                if (packet.ProductId == -3)
                {
                    //Widthdraw produced cubes
                    pool[packet.LabIndex].produced[0] = 0;
                }
                else if (packet.ProductId == -2)
                {
                    //Research recipe reseted
                    pool[packet.LabIndex].SetFunction(false, 0, 0, GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.entitySignPool);
                }
                else if (packet.ProductId == -1)
                {
                    //Center chenged to research-mode
                    pool[packet.LabIndex].SetFunction(true, 0, GameMain.data.history.currentTech, GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.entitySignPool);
                }
                else
                {
                    //Cube Recipe changed
                    int[] matrixIds = LabComponent.matrixIds;
                    RecipeProto recipeProto = LDB.items.Select(matrixIds[packet.ProductId]).maincraft;
                    pool[packet.LabIndex].SetFunction(false, (recipeProto == null) ? 0 : recipeProto.ID, 0, GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.entitySignPool);
                }
                GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factorySystem?.SyncLabFunctions(GameMain.mainPlayer, packet.LabIndex);
            }
        }
    }
}
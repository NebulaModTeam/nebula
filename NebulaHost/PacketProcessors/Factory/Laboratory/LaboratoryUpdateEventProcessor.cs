using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.Laboratory;
using NebulaModel.Packets.Processors;

namespace NebulaHost.PacketProcessors.Factory.Labratory
{
    [RegisterPacketProcessor]
    class LaboratoryUpdateEventProcessor : IPacketProcessor<LaboratoryUpdateEventPacket>
    {
        public void ProcessPacket(LaboratoryUpdateEventPacket packet, NebulaConnection conn)
        {
            LabComponent[] pool = GameMain.localPlanet?.factory?.factorySystem.labPool;
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
                    pool[packet.LabIndex].SetFunction(false, 0, 0, GameMain.localPlanet.factory.entitySignPool);
                }
                else if (packet.ProductId == -1)
                {
                    //Research center mode changed to research-mode
                    pool[packet.LabIndex].SetFunction(true, 0, GameMain.data.history.currentTech, GameMain.localPlanet.factory.entitySignPool);
                }
                else
                {
                    //Cube Recipe changed
                    int[] matrixIds = LabComponent.matrixIds;
                    RecipeProto recipeProto = LDB.items.Select(matrixIds[packet.ProductId]).maincraft;
                    pool[packet.LabIndex].SetFunction(false, (recipeProto == null) ? 0 : recipeProto.ID, 0, GameMain.localPlanet.factory.entitySignPool);
                }
            }
        }
    }
}
#region

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Laboratory;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Labratory;

[RegisterPacketProcessor]
internal class LaboratoryUpdateEventProcessor : PacketProcessor<LaboratoryUpdateEventPacket>
{
    public override void ProcessPacket(LaboratoryUpdateEventPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        var pool = factory?.factorySystem?.labPool;
        if (pool != null && packet.LabIndex != -1 && packet.LabIndex < pool.Length && pool[packet.LabIndex].id != -1)
        {
            using (Multiplayer.Session.Factories.IsIncomingRequest.On())
            {
                Multiplayer.Session.Factories.PacketAuthor = NebulaModAPI.AUTHOR_NONE;
                if (packet.ProductId == -4)
                {
                    pool[packet.LabIndex].forceAccMode = !pool[packet.LabIndex].forceAccMode;
                }
                else if (packet.ProductId == -3)
                {
                    //Widthdraw produced cubes
                    pool[packet.LabIndex].produced[0] = 0;
                }
                else if (packet.ProductId == -2)
                {
                    //Research recipe reseted
                    pool[packet.LabIndex].SetFunction(false, 0, 0, factory.entitySignPool);
                }
                else if (packet.ProductId == -1)
                {
                    //Center chenged to research-mode
                    pool[packet.LabIndex].SetFunction(true, 0, GameMain.data.history.currentTech, factory.entitySignPool);
                }
                else
                {
                    //Cube Recipe changed
                    var matrixIds = LabComponent.matrixIds;
                    var recipeProto = LDB.items.Select(matrixIds[packet.ProductId]).maincraft;
                    pool[packet.LabIndex]
                        .SetFunction(false, recipeProto == null ? 0 : recipeProto.ID, 0, factory.entitySignPool);
                }
                factory.factorySystem?.SyncLabFunctions(GameMain.mainPlayer, packet.LabIndex);
                factory.factorySystem?.SyncLabForceAccMode(GameMain.mainPlayer, packet.LabIndex);
            }
        }
    }
}

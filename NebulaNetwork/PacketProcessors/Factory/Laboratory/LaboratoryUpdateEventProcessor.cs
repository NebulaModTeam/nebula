#region

using NebulaAPI;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Laboratory;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Laboratory;

[RegisterPacketProcessor]
internal class LaboratoryUpdateEventProcessor : PacketProcessor<LaboratoryUpdateEventPacket>
{
    protected override void ProcessPacket(LaboratoryUpdateEventPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        var pool = factory?.factorySystem?.labPool;
        if (pool == null || packet.LabIndex == -1 || packet.LabIndex >= pool.Length || pool[packet.LabIndex].id == -1)
        {
            return;
        }
        using (Multiplayer.Session.Factories.IsIncomingRequest.On())
        {
            Multiplayer.Session.Factories.PacketAuthor = NebulaModAPI.AUTHOR_NONE;
            switch (packet.ProductId)
            {
                case -4:
                    pool[packet.LabIndex].forceAccMode = !pool[packet.LabIndex].forceAccMode;
                    break;
                case -3:
                    //Withdraw produced cubes
                    pool[packet.LabIndex].produced[0] = 0;
                    break;
                case -2:
                    //Research recipe reset
                    pool[packet.LabIndex].SetFunction(false, 0, 0, factory.entitySignPool);
                    break;
                case -1:
                    //Center changed to research-mode
                    pool[packet.LabIndex].SetFunction(true, 0, GameMain.data.history.currentTech, factory.entitySignPool);
                    break;
                default:
                    {
                        //Cube Recipe changed
                        var matrixIds = LabComponent.matrixIds;
                        var recipeProto = LDB.items.Select(matrixIds[packet.ProductId]).maincraft;
                        pool[packet.LabIndex]
                            .SetFunction(false, recipeProto?.ID ?? 0, 0, factory.entitySignPool);
                        break;
                    }
            }
            factory.factorySystem?.SyncLabFunctions(GameMain.mainPlayer, packet.LabIndex);
            factory.factorySystem?.SyncLabForceAccMode(GameMain.mainPlayer, packet.LabIndex);
        }
    }
}

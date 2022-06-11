using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;

namespace NebulaNetwork.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    class EntityBoostSwitchProcessor : PacketProcessor<EntityBoostSwitchPacket>
    {
        public override void ProcessPacket(EntityBoostSwitchPacket packet, NebulaConnection conn)
        {
            PlanetFactory factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
            if (factory == null)
            {
                return;
            }

            switch (packet.EntityType)
            {
                case EBoostEntityType.ArtificialStar:
                    if (packet.Id < factory.powerSystem.genCursor)
                    {
                        factory.powerSystem.genPool[packet.Id].SetBoost(packet.Enable);
                        if (UIRoot.instance.uiGame.generatorWindow.generatorId == packet.Id)
                        {
                            UIRoot.instance.uiGame.generatorWindow.boostSwitch.SetImmediately(packet.Enable);
                        }
                    }
                    break;

                case EBoostEntityType.Ejector:
                    if (packet.Id < factory.factorySystem.ejectorCursor)
                    {
                        factory.factorySystem.ejectorPool[packet.Id].SetBoost(packet.Enable);
                        if (UIRoot.instance.uiGame.ejectorWindow.ejectorId == packet.Id)
                        {
                            UIRoot.instance.uiGame.ejectorWindow.boostSwitch.SetImmediately(packet.Enable);
                        }
                    }
                    break;

                case EBoostEntityType.Silo:
                    if (packet.Id < factory.factorySystem.siloCursor)
                    {
                        factory.factorySystem.siloPool[packet.Id].SetBoost(packet.Enable);
                        if (UIRoot.instance.uiGame.siloWindow.siloId == packet.Id)
                        {
                            UIRoot.instance.uiGame.siloWindow.boostSwitch.SetImmediately(packet.Enable);
                        }
                    }
                    break;
            }
        }
    }
}

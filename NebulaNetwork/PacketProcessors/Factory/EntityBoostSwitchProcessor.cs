#region

using System;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory;

[RegisterPacketProcessor]
internal class EntityBoostSwitchProcessor : PacketProcessor<EntityBoostSwitchPacket>
{
    protected override void ProcessPacket(EntityBoostSwitchPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
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
                        UIRoot.instance.uiGame.generatorWindow.boostSwitch.SetToggleNoEvent(packet.Enable);
                    }
                }
                break;

            case EBoostEntityType.Ejector:
                if (packet.Id < factory.factorySystem.ejectorCursor)
                {
                    factory.factorySystem.ejectorPool[packet.Id].SetBoost(packet.Enable);
                    if (UIRoot.instance.uiGame.ejectorWindow.ejectorId == packet.Id)
                    {
                        UIRoot.instance.uiGame.ejectorWindow.boostSwitch.SetToggleNoEvent(packet.Enable);
                    }
                }
                break;

            case EBoostEntityType.Silo:
                if (packet.Id < factory.factorySystem.siloCursor)
                {
                    factory.factorySystem.siloPool[packet.Id].SetBoost(packet.Enable);
                    if (UIRoot.instance.uiGame.siloWindow.siloId == packet.Id)
                    {
                        UIRoot.instance.uiGame.siloWindow.boostSwitch.SetToggleNoEvent(packet.Enable);
                    }
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(packet), "Unknown EntityBoostSwitchPacket type: " + packet.EntityType);
        }
    }
}

#region

using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.BattleBase;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.BattleBase;

[RegisterPacketProcessor]
internal class BattleBaseSettingUpdateProcessor : PacketProcessor<BattleBaseSettingUpdatePacket>
{
    protected override void ProcessPacket(BattleBaseSettingUpdatePacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (IsHost && factory != null)
        {
            if (packet.Event != BattleBaseSettingEvent.ChangeFleetConfig)
            {
                // Broadcast to the star system where the event planet is located
                Multiplayer.Session.Network.SendPacketToStar(packet, factory.planet.star.id);
            }
            else
            {
                // Don't send back packet to original author because ChangeFleetConfig has been called
                Multiplayer.Session.Network.SendPacketToStarExclude(packet, factory.planet.star.id, conn);
            }
        }

        var pool = factory?.defenseSystem.battleBases;
        if (pool == null || packet.BattleBaseId <= 0)
        {
            return;
        }
        var battleBase = pool[packet.BattleBaseId];

        switch (packet.Event)
        {
            case BattleBaseSettingEvent.ChangeMaxChargePower:
                factory.powerSystem.consumerPool[battleBase.pcId].workEnergyPerTick = (long)(5000.0 * packet.Arg1 + 0.5);
                break;

            case BattleBaseSettingEvent.ToggleDroneEnabled:
                battleBase.constructionModule.droneEnabled = packet.Arg1 != 0f;
                break;

            case BattleBaseSettingEvent.ChangeDronesPriority:
                battleBase.constructionModule.ChangeDronesPriority(factory, (int)packet.Arg1);
                break;

            case BattleBaseSettingEvent.ToggleCombatModuleEnabled:
                battleBase.combatModule.moduleEnabled = packet.Arg1 != 0f;
                break;

            case BattleBaseSettingEvent.ToggleAutoReconstruct:
                battleBase.constructionModule.autoReconstruct = packet.Arg1 != 0f;
                if (battleBase.constructionModule.autoReconstruct)
                {
                    battleBase.constructionModule.SearchAutoReconstructTargets(factory, GameMain.mainPlayer, true);
                }
                break;

            case BattleBaseSettingEvent.ToggleAutoPickEnabled:
                battleBase.autoPickEnabled = packet.Arg1 != 0f;
                break;

            case BattleBaseSettingEvent.ChangeFleetConfig:
                // Copy ChangeFleetConfig without storage/inventory interactions
                // This may change for future ModuleFleet syncing
                const int FleetIndex = 0;
                var newConfigId = (int)packet.Arg1;
                ref var moduleFleet = ref battleBase.combatModule.moduleFleets[FleetIndex];
                for (var i = 0; i < moduleFleet.fighters.Length; i++)
                {
                    if (moduleFleet.fighters[i].itemId == newConfigId)
                    {
                        continue;
                    }
                    if (moduleFleet.fighters[i].count > 0)
                    {
                        var itemId = 0;
                        var count = 0;
                        moduleFleet.TakeFighterFromPort(i, ref itemId, ref count);
                    }
                    moduleFleet.SetItemId(i, newConfigId);
                }
                break;

            case BattleBaseSettingEvent.ToggleAutoReplenishFleet:
                battleBase.combatModule.autoReplenishFleet = packet.Arg1 != 0f;
                break;

            case BattleBaseSettingEvent.None:
            default:
                Log.Warn($"BattleBaseSettingEvent: Unhandled BattleBaseSettingEvent {packet.Event}");
                break;
        }

        //Update UI window too if the local is viewing the current Battle Base
        var battleBaseWindow = UIRoot.instance.uiGame.battleBaseWindow;
        if (battleBaseWindow.battleBaseId != packet.BattleBaseId || battleBaseWindow.factory?.planetId != packet.PlanetId)
        {
            return;
        }
        battleBaseWindow.eventLock = true;
        battleBaseWindow.OnBattleBaseIdChange();
        battleBaseWindow.eventLock = false;
    }
}

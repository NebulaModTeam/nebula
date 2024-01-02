using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.BattleBase;
using NebulaWorld.Player;

namespace NebulaNetwork.PacketProcessors.Factory.BattleBase
{
    [RegisterPacketProcessor]
    internal class NewBattleBaseDroneOrderProcessor : PacketProcessor<NewBattleBaseDroneOrderPacket>
    {
        protected override void ProcessPacket(NewBattleBaseDroneOrderPacket packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                // Host knows about all factories and tells clients when to eject drones from battlebases
            }
            else
            {
                if (packet.PlanetId != GameMain.mainPlayer.planetId)
                {
                    return;
                }

                PlanetFactory factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
                if (factory != null)
                {
                    BattleBaseComponent battleBaseComponent = factory.defenseSystem.battleBases.buffer[packet.Owner];
                    double droneEjectEnergy = Configs.freeMode.droneEjectEnergy;

                    DroneComponent[] dronesBuffer = factory.constructionSystem.drones.buffer;
                    int cursor = factory.constructionSystem.drones.cursor;
                    for (int i = 1; i < cursor; i++)
                    {
                        ref DroneComponent drone = ref dronesBuffer[i];
                        if (drone.id == i && drone.owner == packet.Owner && drone.stage == 0) // checking for stage 0 is important to not reuse the same drone again and again.
                        {
                            ref CraftData craftData = ref factory.craftPool[drone.craftId];

                            if (packet.IsConstruction && factory.constructionSystem.TakeEnoughItemsFromBase(battleBaseComponent, packet.EntityId))
                            {
                                battleBaseComponent.constructionModule.EjectBaseDrone(factory, ref drone, ref craftData, packet.EntityId);
                                battleBaseComponent.energy -= (long)droneEjectEnergy;
                                factory.constructionSystem.constructServing.Add(packet.EntityId);

                                // TODO: needed?
                                //factory.constructionSystem.constructServing.Add(packet.EntityId);
                                return;
                            }
                            if (!packet.IsConstruction)
                            {
                                battleBaseComponent.constructionModule.EjectBaseDrone(factory, ref drone, ref craftData, packet.EntityId);
                                battleBaseComponent.energy -= (long)droneEjectEnergy;

                                /* TODO: needed?
                                ref EntityData ptr5 = ref factory.entityPool[packet.EntityId];
                                CombatStat[] buffer4 = factory.skillSystem.combatStats.buffer;
                                int combatStatId3 = ptr5.combatStatId;
                                buffer4[combatStatId3].repairerCount = buffer4[combatStatId3].repairerCount + 1;
                                */
                                return;
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
}

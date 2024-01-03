using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.BattleBase;

namespace NebulaNetwork.PacketProcessors.Factory.BattleBase;

[RegisterPacketProcessor]
internal class NewBattleBaseDroneOrderProcessor : PacketProcessor<NewBattleBaseDroneOrderPacket>
{
    protected override void ProcessPacket(NewBattleBaseDroneOrderPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            // Host knows about all factories and tells clients when to eject drones from battlebases
            return;
        }
        if (packet.PlanetId != GameMain.mainPlayer.planetId)
        {
            return;
        }

        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null)
        {
            return;
        }
        var battleBaseComponent = factory.defenseSystem.battleBases.buffer[packet.Owner];
        var droneEjectEnergy = Configs.freeMode.droneEjectEnergy;

        var dronesBuffer = factory.constructionSystem.drones.buffer;
        var cursor = factory.constructionSystem.drones.cursor;
        for (var i = 1; i < cursor; i++)
        {
            ref var drone = ref dronesBuffer[i];
            if (drone.id != i || drone.owner != packet.Owner ||
                drone.stage != 0) // checking for stage 0 is important to not reuse the same drone again and again.
            {
                continue;
            }
            ref var craftData = ref factory.craftPool[drone.craftId];

            switch (packet.IsConstruction)
            {
                case true when
                    factory.constructionSystem.TakeEnoughItemsFromBase(battleBaseComponent, packet.EntityId):
                    battleBaseComponent.constructionModule.EjectBaseDrone(factory, ref drone, ref craftData,
                        packet.EntityId);
                    battleBaseComponent.energy -= (long)droneEjectEnergy;
                    factory.constructionSystem.constructServing.Add(packet.EntityId);

                    return;
                case false:
                    battleBaseComponent.constructionModule.EjectBaseDrone(factory, ref drone, ref craftData,
                        packet.EntityId);
                    battleBaseComponent.energy -= (long)droneEjectEnergy;

                    /* TODO: needed?
                            ref EntityData ptr5 = ref factory.entityPool[packet.EntityId];
                            CombatStat[] buffer4 = factory.skillSystem.combatStats.buffer;
                            int combatStatId3 = ptr5.combatStatId;
                            buffer4[combatStatId3].repairerCount = buffer4[combatStatId3].repairerCount + 1;
                            */
                    break;
            }
            break;
        }
    }
}

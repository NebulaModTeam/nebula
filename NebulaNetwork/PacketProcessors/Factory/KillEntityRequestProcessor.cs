#region

using NebulaAPI;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;
using NebulaWorld;
using NebulaWorld.Factory;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory;

[RegisterPacketProcessor]
public class KillEntityRequestProcessor : PacketProcessor<KillEntityRequest>
{
    protected override void ProcessPacket(KillEntityRequest packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

        using (Multiplayer.Session.Factories.IsIncomingRequest.On())
        {
            Multiplayer.Session.Factories.TargetPlanet = packet.PlanetId;
            Multiplayer.Session.Factories.EventFactory = factory;

            if (!factory.planet.factoryLoaded)
            {
                // If planet is remote planet or not yet loaded, remove planet.physics that added by planetTimer
                var factoryManager = Multiplayer.Session.Factories as FactoryManager;
                if (factoryManager.RemovePlanetTimer(packet.PlanetId))
                {
                    factoryManager.UnloadPlanetData(packet.PlanetId);
                }
            }

            ref var entityPtr = ref factory.entityPool[packet.ObjId];
            if (entityPtr.id == packet.ObjId)
            {
                factory.KillEntityFinally(GameMain.mainPlayer, packet.ObjId, ref CombatStat.empty);
            }

            Multiplayer.Session.Factories.TargetPlanet = NebulaModAPI.PLANET_NONE;
            Multiplayer.Session.Factories.EventFactory = null;
        }
    }
}

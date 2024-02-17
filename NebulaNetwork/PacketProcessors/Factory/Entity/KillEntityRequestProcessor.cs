#region

using NebulaAPI;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Entity;

[RegisterPacketProcessor]
public class KillEntityRequestProcessor : PacketProcessor<KillEntityRequest>
{
    protected override void ProcessPacket(KillEntityRequest packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null)
        {
            return;
        }

        using (Multiplayer.Session.Factories.IsIncomingRequest.On())
        {
            Multiplayer.Session.Factories.TargetPlanet = packet.PlanetId;

            ref var entityPtr = ref factory.entityPool[packet.ObjId];
            if (entityPtr.id == packet.ObjId)
            {
                factory.KillEntityFinally(GameMain.mainPlayer, packet.ObjId, ref CombatStat.empty);
            }

            Multiplayer.Session.Factories.TargetPlanet = NebulaModAPI.PLANET_NONE;
        }
    }
}

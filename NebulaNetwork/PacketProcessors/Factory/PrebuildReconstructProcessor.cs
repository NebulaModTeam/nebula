#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory;

[RegisterPacketProcessor]
public class PrebuildReconstructProcessor : PacketProcessor<PrebuildReconstructPacket>
{
    protected override void ProcessPacket(PrebuildReconstructPacket packet, NebulaConnection conn)
    {
        var planet = GameMain.galaxy.PlanetById(packet.PlanetId);
        if (planet.factory == null) return;
        var factory = planet.factory;

        ref var ptr = ref factory.prebuildPool[packet.PrebuildId];
        if (ptr.id != packet.PrebuildId)
        {
            //Prebuild not exist
            return;
        }
        if (IsHost)
        {
            var starId = planet.star.id;
            Multiplayer.Session.Network.SendPacketToStarExclude(packet, starId, conn);
        }

        using (Multiplayer.Session.Factories.IsIncomingRequest.On())
        {
            factory.ReconstructTargetFinally(packet.PrebuildId);
        }
    }
}

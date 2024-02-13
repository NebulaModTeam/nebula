#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class DFGRemoveBasePitProcessor : PacketProcessor<DFGRemoveBasePitPacket>
{
    protected override void ProcessPacket(DFGRemoveBasePitPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

        if (IsHost) // Forward and approve the active base event
        {
            Multiplayer.Session.Network.SendPacketToStar(packet, factory.planet.star.id);
        }

        using (Multiplayer.Session.Combat.IsIncomingRequest.On())
        {
            //Use RemoveBase instead for it uses baseId directly
            factory.enemySystem.RemoveBase(packet.BaseId, true);
        }
        GameMain.gameScenario.NotifyOnRemovePit();
    }
}

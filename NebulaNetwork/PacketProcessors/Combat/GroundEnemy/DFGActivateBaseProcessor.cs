#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class DFGActivateBaseProcessor : PacketProcessor<DFGActivateBasePacket>
{
    protected override void ProcessPacket(DFGActivateBasePacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

        if (IsHost) // Forward and approve the active base event
        {
            Multiplayer.Session.Network.SendPacketToStar(packet, factory.planet.star.id);
        }

        if (!packet.SetToSeekForm)
        {
            if (packet.BaseId >= factory.enemySystem.bases.capacity) return;
            var dFBase = factory.enemySystem.bases.buffer[packet.BaseId];
            if (dFBase == null) return;
            dFBase.activeTick = 3;
            using (Multiplayer.Session.Combat.IsIncomingRequest.On())
            {
                dFBase.ActiveAllUnit(GameMain.gameTick);
            }
        }
        else
        {
            ref var buffer = ref factory.enemySystem.units.buffer;
            var cursor = factory.enemySystem.units.cursor;
            for (var i = 1; i < cursor; i++)
            {
                if (buffer[i].baseId == packet.BaseId && buffer[i].behavior == EEnemyBehavior.KeepForm)
                {
                    buffer[i].behavior = EEnemyBehavior.SeekForm;
                }
            }
        }
    }
}

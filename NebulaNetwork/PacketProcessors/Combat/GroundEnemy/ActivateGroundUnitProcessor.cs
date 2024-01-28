#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class ActivateGroundUnitProcessor : PacketProcessor<ActivateGroundUnitPacket>
{
    protected override void ProcessPacket(ActivateGroundUnitPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

        var gameTick = GameMain.gameTick;
        var enemyId = factory.enemySystem.ActivateUnit(packet.BaseId, packet.FormId, packet.PortId, gameTick);
        ref var buffer = ref factory.enemySystem.units.buffer;
        buffer[enemyId].behavior = (EEnemyBehavior)packet.Behavior;
        buffer[enemyId].stateTick = packet.StateTick;
    }
}

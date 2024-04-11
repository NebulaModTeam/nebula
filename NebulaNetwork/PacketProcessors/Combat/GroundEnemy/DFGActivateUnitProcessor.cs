#region

using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;
using NebulaWorld.Combat;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class DFGActivateUnitProcessor : PacketProcessor<DFGActivateUnitPacket>
{
    protected override void ProcessPacket(DFGActivateUnitPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

        using (Multiplayer.Session.Combat.IsIncomingRequest.On())
        {
            EnemyManager.SetPlanetFactoryNextEnemyId(factory, packet.EnemyId);
            if (packet.BaseId >= factory.enemySystem.bases.capacity) return;
            var dfBase = factory.enemySystem.bases.buffer[packet.BaseId];
            if (dfBase == null) return;
            var gameTick = GameMain.gameTick;

            // the value inside enemyFormation.units[portId] is not reliable, so just overwrite it
            var enemyFormation = dfBase.forms[packet.FormId];
            enemyFormation.units[packet.PortId] = 1;
            var unitId = factory.enemySystem.ActivateUnit(packet.BaseId, packet.FormId, packet.PortId, gameTick);

#if DEBUG
            if (unitId == 0)
            {
                Log.Warn($"DFSActivateUnitPacket unitId = 0!");
                return;
            }
            var enemyId = factory.enemySystem.units.buffer[unitId].enemyId;
            if (enemyId != packet.EnemyId)
            {
                Log.Warn($"DFSActivateUnitPacket enemyId mismatch! {packet.EnemyId} => {enemyId}");
            }
#endif

            ref var enemyUnit = ref factory.enemySystem.units.buffer[unitId];
            enemyUnit.behavior = (EEnemyBehavior)packet.Behavior;
            enemyUnit.stateTick = packet.StateTick;
        }
    }
}

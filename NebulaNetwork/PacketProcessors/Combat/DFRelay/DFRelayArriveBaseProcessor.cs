#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.DFRelay;
using NebulaWorld;
using NebulaWorld.Combat;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.DFRelay;

[RegisterPacketProcessor]
public class DFRelayArriveBaseProcessor : PacketProcessor<DFRelayArriveBasePacket>
{
    protected override void ProcessPacket(DFRelayArriveBasePacket packet, NebulaConnection conn)
    {
        var hiveSystem = GameMain.spaceSector.GetHiveByAstroId(packet.HiveAstroId);
        if (hiveSystem == null) return;

        var dfrelayComponent = hiveSystem.relays.buffer[packet.RelayId];
        if (dfrelayComponent?.id != packet.RelayId) return;

        ref var enemyData = ref GameMain.spaceSector.enemyPool[dfrelayComponent.enemyId];
        ref var animData = ref GameMain.spaceSector.enemyAnimPool[dfrelayComponent.enemyId];
        dfrelayComponent.param0 = 0f;
        enemyData.pos = dfrelayComponent.targetLPos;
        enemyData.rot = Maths.SphericalRotation(dfrelayComponent.targetLPos, dfrelayComponent.targetYaw * 360f);
        animData.time = 1f;

        using (Multiplayer.Session.Enemies.IsIncomingRelayRequest.On())
        {
            hiveSystem.rtseed = packet.HiveRtseed;
            dfrelayComponent.stage = 2;
            using (Multiplayer.Session.Combat.IsIncomingRequest.On())
            {
                var factory = dfrelayComponent.hive.galaxy.astrosFactory[dfrelayComponent.targetAstroId];
                if (factory != null)
                {
                    // Prepare enemyId in CreateEnemyPlanetBase
                    EnemyManager.SetPlanetFactoryNextEnemyId(factory, packet.NextGroundEnemyId);
                }
                dfrelayComponent.ArriveBase();
            }
        }
    }
}

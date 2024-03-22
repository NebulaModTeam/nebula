#region

using NebulaAPI.DataStructures;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.DFRelay;
using NebulaWorld;
using NebulaWorld.Combat;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.DFRelay;

[RegisterPacketProcessor]
public class DFRelayRealizePlanetBaseProcessor : PacketProcessor<DFRelayRealizePlanetBasePacket>
{
    protected override void ProcessPacket(DFRelayRealizePlanetBasePacket packet, NebulaConnection conn)
    {
        var hiveSystem = GameMain.spaceSector.GetHiveByAstroId(packet.HiveAstroId);
        if (hiveSystem == null) return;

        var dfrelayComponent = hiveSystem.relays.buffer[packet.RelayId];
        if (dfrelayComponent?.id != packet.RelayId) return;

        dfrelayComponent.hive.relayNeutralizedCounter = packet.RelayNeutralizedCounter;
        dfrelayComponent.hive.seed = packet.HiveSeed;
        dfrelayComponent.hive.rtseed = packet.HiveRtseed;
        dfrelayComponent.targetAstroId = packet.TargetAstroId;
        dfrelayComponent.targetLPos = packet.TargetLPos.ToVector3();
        dfrelayComponent.targetYaw = packet.TargetYaw;
        dfrelayComponent.baseTicks = packet.BaseTicks;

        using (Multiplayer.Session.Enemies.IsIncomingRelayRequest.On())
        {
            using (Multiplayer.Session.Combat.IsIncomingRequest.On())
            {
                var factory = dfrelayComponent.hive.galaxy.astrosFactory[dfrelayComponent.targetAstroId];
                if (factory != null && packet.EnemyCursor != -1)
                {
                    // Prepare enemyIds in CreateEnemyPlanetBase
                    EnemyManager.SetPlanetFactoryRecycle(factory, packet.EnemyCursor, packet.EnemyRecyle);
                }
                dfrelayComponent.RealizePlanetBase(GameMain.spaceSector);
            }
        }
    }
}

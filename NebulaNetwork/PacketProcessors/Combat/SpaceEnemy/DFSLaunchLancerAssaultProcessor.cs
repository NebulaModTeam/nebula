#region

using NebulaAPI.DataStructures;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.SpaceEnemy;
using NebulaWorld;
using NebulaWorld.Combat;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.SpaceEnemy;

[RegisterPacketProcessor]
public class DFSLaunchLancerAssaultProcessor : PacketProcessor<DFSLaunchLancerAssaultPacket>
{
    protected override void ProcessPacket(DFSLaunchLancerAssaultPacket packet, NebulaConnection conn)
    {
        var spaceSector = GameMain.spaceSector;
        var hive = spaceSector.GetHiveByAstroId(packet.HiveAstroId);
        if (hive == null) return;

        using (Multiplayer.Session.Enemies.IsIncomingRequest.On())
        {
            // Set enemyRecycle pool to make enemyId stay in sync
            EnemyManager.SetSpaceSectorRecycle(packet.EnemyCursor, packet.EnemyRecycle);

            // Modify from EnemyDFHiveSystem.AssaultingWavesDetermineAI
            var aggressiveLevel = (EAggressiveLevel)packet.AggressiveLevel;
            hive.turboTicks = 120;
            hive.turboRepress = 0;
            hive.evolve.threat = packet.EvolveThreat;
            hive.LaunchLancerAssault(aggressiveLevel, packet.TarPos.ToVector3(), packet.MaxHatredPos.ToVector3(),
                packet.TargetAstroId, packet.UnitCount0, packet.UnitThreat);
            hive.evolve.threat = 0;
            hive.evolve.threatshr = 0;
            hive.evolve.maxThreat = EvolveData.GetSpaceThreatMaxByWaves(hive.evolve.waves, aggressiveLevel);
            hive.lancerAssaultCountBase += hive.GetLancerAssaultCountIncrement(aggressiveLevel);
        }
    }
}

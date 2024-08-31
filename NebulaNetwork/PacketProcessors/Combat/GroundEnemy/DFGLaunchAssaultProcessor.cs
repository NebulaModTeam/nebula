#region

using NebulaAPI.DataStructures;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;
using NebulaWorld.Combat;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class DFGLaunchAssaultProcessor : PacketProcessor<DFGLaunchAssaultPacket>
{
    protected override void ProcessPacket(DFGLaunchAssaultPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null)
        {
            return;
        }

        using (Multiplayer.Session.Combat.IsIncomingRequest.On())
        {
            // Set enemyRecycle pool to make enemyId stay in sync
            EnemyManager.SetPlanetFactoryRecycle(factory, packet.EnemyCursor, packet.EnemyRecyle);

            if (packet.BaseId >= factory.enemySystem.bases.capacity) return;
            var dFBase = factory.enemySystem.bases.buffer[packet.BaseId];
            if (dFBase == null) return;
            dFBase.turboTicks = 60;
            dFBase.turboRepress = 0;
            dFBase.evolve.threat = packet.EvolveThreat;
            dFBase.LaunchAssault(packet.TarPos.ToVector3(), packet.ExpandRadius, packet.UnitCount0, packet.UnitCount1,
                packet.Ap0, packet.Ap1, packet.UnitThreat);
            dFBase.evolve.threat = 0;
            dFBase.evolve.threatshr = 0;
            var agglv = GameMain.history.combatSettings.aggressiveLevel;
            dFBase.evolve.maxThreat = EvolveData.GetGroundThreatMaxByWaves(dFBase.evolve.waves, agglv);
        }
    }
}

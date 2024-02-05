#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Turret;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Turret;

[RegisterPacketProcessor]
internal class TurretPhaseUpdateProcessor : PacketProcessor<TurretPhaseUpdatePacket>
{
    protected override void ProcessPacket(TurretPhaseUpdatePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.defenseSystem.turrets;
        if (pool == null || packet.TurretId < 0 || packet.TurretId >= pool.buffer.Length)
        {
            return;
        }
        ref var turret = ref pool.buffer[packet.TurretId];
        turret.phasePos = packet.PhasePos;

        // Refresh UI if viewing on the same turret
        var uiTurret = UIRoot.instance.uiGame.turretWindow;
        if (uiTurret.factory == null || uiTurret.factory.planetId != packet.PlanetId || uiTurret.turretId != packet.TurretId)
        {
            return;
        }
        uiTurret.phaseText.text = (turret.phasePos / 60f).ToString("0.##");
    }
}

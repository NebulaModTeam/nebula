#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Turret;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Turret;

[RegisterPacketProcessor]
internal class TurretGroupUpdateProcessor : PacketProcessor<TurretGroupUpdatePacket>
{
    protected override void ProcessPacket(TurretGroupUpdatePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.defenseSystem.turrets;
        if (pool == null || packet.TurretIndex == -1 || packet.TurretIndex >= pool.buffer.Length)
        {
            return;
        }
        ref var turret = ref pool.buffer[packet.TurretIndex];
        if (turret.id != -1)
        {
            turret.SetGroup(packet.Group);
        }

        // Refresh UI if viewing on the same turret
        var uiTurret = UIRoot.instance.uiGame.turretWindow;
        if (uiTurret.factory == null || uiTurret.factory.planetId != packet.PlanetId || uiTurret.turretId != packet.TurretIndex)
        {
            return;
        }
        for (var i = 0; i < uiTurret.groupSelectionBtns.Length; i++)
        {
            var uibutton = uiTurret.groupSelectionBtns[i];
            uibutton.highlighted = uibutton.data == (int)turret.group;
        }
    }
}

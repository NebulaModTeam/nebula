#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Turret;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Turret;

[RegisterPacketProcessor]
internal class TurretBurstUpdateProcessor : PacketProcessor<TurretBurstUpdatePacket>
{
    protected override void ProcessPacket(TurretBurstUpdatePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.defenseSystem.turrets;
        if (pool == null || packet.TurretIndex == -1)
        {
            return;
        }
        UITurretWindow.burstModeIndex = packet.BurstIndex;

        //Update UI Panel too if it is viewing any turret window
        var uiTurret = UIRoot.instance.uiGame.turretWindow;
        if (uiTurret.factory == null || uiTurret.factory.planetId != packet.PlanetId)
        {
            return;
        }

        uiTurret.RefreshBurstModeUI();
    }
}

#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Turret;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Turret;

[RegisterPacketProcessor]
internal class TurretSuperNovaProcessor : PacketProcessor<TurretSuperNovaPacket>
{
    protected override void ProcessPacket(TurretSuperNovaPacket packet, NebulaConnection conn)
    {
        var defenseSystem = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.defenseSystem;
        var pool = defenseSystem?.turrets;
        if (pool == null || packet.TurretIndex == -1 || packet.TurretIndex >= pool.buffer.Length ||
            pool.buffer[packet.TurretIndex].id == -1)
        {
            return;
        }
        //TODO: Evaluate in PR, should I count on other packet, or should I pass through?
        var burstModeIndex = UITurretWindow.burstModeIndex;
        var inSuperNova = packet.InSuperNova;

        var refTurret = pool.buffer[packet.TurretIndex];

        switch (burstModeIndex)
        {
            case 1:
                if (inSuperNova)
                {
                    refTurret.SetSupernova();
                }
                else
                {
                    refTurret.CancelSupernova();
                }
                break;
            case 2:
                if (inSuperNova)
                {
                    defenseSystem.SetGroupTurretsSupernova(refTurret.group);
                }
                else
                {
                    defenseSystem.CancelGroupTurretSupernova(refTurret.group);
                }
                break;
            case 3:
                if (inSuperNova)
                {
                    defenseSystem.SetGlobalTurretsSupernova();
                }
                else
                {
                    defenseSystem.CancelGlobalTurretSupernova();
                }
                break;
        }
    }
}

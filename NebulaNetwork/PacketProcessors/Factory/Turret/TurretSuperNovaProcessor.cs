#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Ejector;
using NebulaModel.Packets.Factory.Turret;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Ejector;

[RegisterPacketProcessor]
internal class TurretSuperNovaProcessor : PacketProcessor<TurretSuperNovaPacket>
{
    protected override void ProcessPacket(TurretSuperNovaPacket packet, NebulaConnection conn)
    {
        var defenseSytem = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.defenseSystem;
        var pool = defenseSytem.turrets;
        if (pool != null && packet.TurretIndex != -1 && packet.TurretIndex < pool.buffer.Length &&
            pool.buffer[packet.TurretIndex].id != -1)
        {
            //TODO: Evaluate in PR, should I count on other packet, or should I pass through?
            int burstModeIndex = UITurretWindow.burstModeIndex;
            bool inSuperNova = packet.InSuperNova;

            var refTurret = pool.buffer[packet.TurretIndex];

            switch (burstModeIndex)
            {
                case 1:
                    if (inSuperNova)
                        refTurret.SetSupernova();
                    else
                        refTurret.CancelSupernova();
                    break;
                case 2:
                    if (inSuperNova)
                        defenseSytem.SetGroupTurretsSupernova(refTurret.group);
                    else
                        defenseSytem.CancelGroupTurretSupernova(refTurret.group);
                    break;
                case 3:
                    if (inSuperNova)
                        defenseSytem.SetGlobalTurretsSupernova();
                    else
                        defenseSytem.CancelGlobalTurretSupernova();
                    break;
            }
        }
    }
}

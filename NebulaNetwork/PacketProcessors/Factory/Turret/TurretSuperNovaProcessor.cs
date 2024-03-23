#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Turret;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Turret;

[RegisterPacketProcessor]
internal class TurretSuperNovaProcessor : PacketProcessor<TurretSuperNovaPacket>
{
    protected override void ProcessPacket(TurretSuperNovaPacket packet, NebulaConnection conn)
    {
        var planet = GameMain.galaxy.PlanetById(packet.PlanetId);
        var defenseSystem = planet?.factory?.defenseSystem;
        var pool = defenseSystem?.turrets;
        if (pool == null || packet.TurretIndex == -1 || packet.TurretIndex >= pool.buffer.Length ||
            pool.buffer[packet.TurretIndex].id == -1)
        {
            return;
        }

        if (IsHost)
        {
            // Broadcast supernova events to other players in the system
            var starId = planet.star.id;
            Multiplayer.Session.Network.SendPacketToStar(packet, starId);
        }

        var setSuperNova = packet.SetSuperNova;
        UITurretWindow.burstModeIndex = packet.BrustModeIndex; // Leave a mark in UI
        ref var refTurret = ref pool.buffer[packet.TurretIndex];
        switch (packet.BrustModeIndex)
        {
            case 1:
                if (setSuperNova)
                {
                    refTurret.SetSupernova();
                }
                else
                {
                    refTurret.CancelSupernova();
                }
                break;
            case 2:
                if (setSuperNova)
                {
                    defenseSystem.SetGroupTurretsSupernova(refTurret.group);
                }
                else
                {
                    defenseSystem.CancelGroupTurretSupernova(refTurret.group);
                }
                break;
            case 3:
                if (setSuperNova)
                {
                    defenseSystem.SetGlobalTurretsSupernova();
                }
                else
                {
                    defenseSystem.CancelGlobalTurretSupernova();
                }
                break;
        }

        var uiTurret = UIRoot.instance.uiGame.turretWindow;
        if (uiTurret.factory == null || uiTurret.factory.planetId != packet.PlanetId || uiTurret.turretId != packet.TurretIndex)
        {
            return;
        }
        if (refTurret.inSupernova)
        {
            GameMain.gameScenario.NotifyOnSupernovaUITriggered();
        }
    }
}

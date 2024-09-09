#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.Mecha;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.Mecha;

[RegisterPacketProcessor]
public class MechaAliveEventProcessor : PacketProcessor<MechaAliveEventPacket>
{
    protected override void ProcessPacket(MechaAliveEventPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            Multiplayer.Session.Server.SendPacketExclude(packet, conn);
        }

        using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
        {
            if (!remotePlayersModels.TryGetValue(packet.PlayerId, out var playerModel)) return;

            switch (packet.Status)
            {
                case MechaAliveEventPacket.EStatus.Kill:
                    var player = playerModel.PlayerInstance;
                    player.isAlive = false;
                    player.deathCount++;
                    player.timeSinceKilled = 0;
                    player.invincibleTicks = 0;
                    player.mecha.Kill();
                    player.mechaArmorModel.Kill();
                    return;

                case MechaAliveEventPacket.EStatus.RespawnAtOnce:
                    playerModel.PlayerInstance.controller.actionDeath.ResetRespawnState();
                    playerModel.PlayerInstance.controller.actionDeath.Respawn(1);
                    return;

                case MechaAliveEventPacket.EStatus.RespawnKeepPosition:
                    // Play respawning animation
                    playerModel.PlayerInstance.controller.actionDeath.ResetRespawnState();
                    playerModel.PlayerInstance.controller.actionDeath.Respawn(2);
                    return;

                case MechaAliveEventPacket.EStatus.RespawnAtBirthPoint:
                    // This is respawnMode3, but due to it will open loading screen and throw trash, we just use respawn 1
                    playerModel.PlayerInstance.controller.actionDeath.ResetRespawnState();
                    playerModel.PlayerInstance.controller.actionDeath.Respawn(1);
                    return;
            }
        }
    }
}

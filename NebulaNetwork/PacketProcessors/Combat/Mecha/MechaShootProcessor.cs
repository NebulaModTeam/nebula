#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.Mecha;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.Mecha;

[RegisterPacketProcessor]
public class MechaShootProcessor : PacketProcessor<MechaShootPacket>
{
    protected override void ProcessPacket(MechaShootPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            var combatManager = Multiplayer.Session.Combat;
            if (combatManager.IndexByPlayerId.TryGetValue(packet.PlayerId, out var index))
            {
                var starId = combatManager.Players[index].starId;
                if (starId > 0)
                {
                    Multiplayer.Session.Server.SendPacketToStarExclude(packet, starId, conn);
                }
                else
                {
                    Multiplayer.Session.Server.SendPacketExclude(packet, conn);
                }
            }
        }

        using (Multiplayer.Session.Combat.IsIncomingRequest.On())
        {
            // Skip combatStat.lastImpact syncing for now
            Multiplayer.Session.Combat.ShootTarget(packet.PlayerId, packet.AmmoItemId, (EAmmoType)packet.AmmoType, packet.TargetAstroId, packet.TargetId);
        }
    }
}

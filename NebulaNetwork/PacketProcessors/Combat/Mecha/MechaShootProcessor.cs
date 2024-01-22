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
        using (Multiplayer.Session.Combat.IsIncomingRequest.On())
        {
            // Skip combatStat.lastImpact syncing for now
            Multiplayer.Session.Combat.ShootTarget(packet.PlayerId, packet.AmmoItemId, (EAmmoType)packet.AmmoType, packet.TargetAstroId, packet.TargetId);
        }
    }
}

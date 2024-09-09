#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat;

[RegisterPacketProcessor]
public class CombatStatDamageProcessor : PacketProcessor<CombatStatDamagePacket>
{
    protected override void ProcessPacket(CombatStatDamagePacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            if (packet.TargetAstroId > 1000000)
            {
                Multiplayer.Session.Server.SendPacketExclude(packet, conn);
            }
        }

        SkillTarget target;
        target.type = (ETargetType)packet.TargetType;
        target.id = packet.TargetId;
        target.astroId = packet.TargetAstroId;
        SkillTarget caster;
        caster.type = (ETargetType)packet.CasterType;
        caster.id = packet.CasterId;
        caster.astroId = packet.CasterAstroId;

        if (target.type != ETargetType.Enemy) return; // Guard

        var astroId = target.astroId;
        if (astroId > 1000000)
        {
            if (target.id >= GameMain.spaceSector.enemyPool.Length)
            {
                // Return if enemyId is not exist in client
                return;
            }
        }
        else if (astroId > 100 && astroId <= 204899 && astroId % 100 > 0)
        {
            var factory = GameMain.spaceSector.skillSystem.astroFactories[astroId];
            if (factory == null || target.id >= factory.enemyPool.Length) return;
        }

        using (Multiplayer.Session.Combat.IsIncomingRequest.On())
        {
            var skillSystem = GameMain.spaceSector.skillSystem;
            var tmp = skillSystem.playerAlive;
            skillSystem.playerAlive = true;
            skillSystem.DamageObject(packet.Damage, packet.Slice, ref target, ref caster);
            skillSystem.playerAlive = tmp;
        }
    }
}

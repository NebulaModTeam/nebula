#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat;

[RegisterPacketProcessor]
public class CombatStatFullHpProcessor : PacketProcessor<CombatStatFullHpPacket>
{
    protected override void ProcessPacket(CombatStatFullHpPacket packet, NebulaConnection conn)
    {
        var combatStatId = 0;
        var starId = 0;
        if (packet.OriginAstroId > 100 && packet.OriginAstroId <= 204899 && packet.OriginAstroId % 100 > 0)
        {
            var planetFactory = GameMain.spaceSector.skillSystem.astroFactories[packet.OriginAstroId];
            if (planetFactory != null)
            {
                starId = packet.OriginAstroId / 100;

                if (packet.ObjectType == 0 && packet.ObjectId < planetFactory.entityPool.Length)
                {
                    combatStatId = planetFactory.entityPool[packet.ObjectId].combatStatId;
                    planetFactory.entityPool[packet.ObjectId].combatStatId = 0;
                }
            }
        }

        var skillSystem = GameMain.spaceSector.skillSystem;
        if (combatStatId > 0 && combatStatId < skillSystem.combatStats.cursor)
        {
            ref var combatStatRef = ref skillSystem.combatStats.buffer[combatStatId];
            if (combatStatRef.id == combatStatId)
            {
                if (IsHost && combatStatRef.warningId > 0)
                {
                    GameMain.data.warningSystem.RemoveWarningData(combatStatRef.warningId);
                }
                skillSystem.OnRemovingSkillTarget(combatStatRef.id, combatStatRef.originAstroId, ETargetType.CombatStat);
                skillSystem.combatStats.Remove(combatStatRef.id);
            }
        }

        if (IsHost)
        {
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
}

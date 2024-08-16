#region

using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class DFGFormationAddUnitProcessor : PacketProcessor<DFGFormationAddUnitPacket>
{
    protected override void ProcessPacket(DFGFormationAddUnitPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

        if (packet.BaseId >= factory.enemySystem.bases.capacity) return;
        var dFBase = factory.enemySystem.bases.buffer[packet.BaseId];
        if (dFBase == null) return;
        using (Multiplayer.Session.Combat.IsIncomingRequest.On())
        {
            // Set the next id in EnemyFormation
            var enemyFrom = dFBase.forms[packet.FormId];

            if (enemyFrom.vacancyCursor <= 0)
            {
                // If vacancyCursor desync, abort the AddUnit action
                Log.Warn($"DFGFormationAddUnitPacket vacancyCursor desync. Pid={packet.PlanetId} Base={packet.BaseId} FormId={packet.FormId}");
                return;
            }
            enemyFrom.vacancies[enemyFrom.vacancyCursor - 1] = packet.PortId;

            var portId = enemyFrom.AddUnit();
#if DEBUG
            if (portId != packet.PortId)
            {
                NebulaModel.Logger.Log.Warn($"DFGFormationAddUnitPacket wrong id {packet.PortId} => {portId}");
            }
#endif
        }
    }
}

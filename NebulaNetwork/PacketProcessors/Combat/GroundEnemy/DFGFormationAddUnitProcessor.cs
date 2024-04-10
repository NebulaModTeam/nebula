#region

using NebulaAPI.Packets;
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

        var dFBase = factory.enemySystem.bases.buffer[packet.BaseId];
        if (dFBase == null) return;
        using (Multiplayer.Session.Combat.IsIncomingRequest.On())
        {
            // Set the next id in EnemyFormation
            var enemyFrom = dFBase.forms[packet.FormId];
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

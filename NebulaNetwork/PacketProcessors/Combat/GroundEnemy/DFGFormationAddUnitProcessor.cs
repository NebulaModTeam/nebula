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
        dFBase.forms[packet.FormId].AddUnit();
    }
}

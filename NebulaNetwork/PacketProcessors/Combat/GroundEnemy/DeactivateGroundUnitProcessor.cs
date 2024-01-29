#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class DeactivateGroundUnitProcessor : PacketProcessor<DeactivateGroundUnitPacket>
{
    protected override void ProcessPacket(DeactivateGroundUnitPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

        using (Multiplayer.Session.Combat.IsIncomingRequest.On())
        {
            factory.enemySystem.DeactivateUnit(packet.UnitId);
        }
    }
}

#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class DFGRemoveBaseProcessor : PacketProcessor<DFGRemoveBasePacket>
{
    protected override void ProcessPacket(DFGRemoveBasePacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (IsHost || factory == null) return;

        using (Multiplayer.Session.Combat.IsIncomingRequest.On())
        {
            // EnemyDFGroundSystem.bases.SetNull(id);
            factory.enemySystem.RemoveDFGBaseComponent(packet.BaseId);
        }
    }
}

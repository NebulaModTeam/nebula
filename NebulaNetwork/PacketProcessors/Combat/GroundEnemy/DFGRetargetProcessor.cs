#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class DFGRetargetProcessor : PacketProcessor<DFGRetargetPacket>
{
    protected override void ProcessPacket(DFGRetargetPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

        var targets = Multiplayer.Session.Enemies.GroundTargets[packet.PlanetId];
        if (packet.EnemyId <= targets.Length)
        {
            targets[packet.EnemyId] = packet.Target;
        }
    }
}

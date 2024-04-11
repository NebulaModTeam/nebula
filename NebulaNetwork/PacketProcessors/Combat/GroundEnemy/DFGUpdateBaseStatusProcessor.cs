#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Combat.GroundEnemy;

#endregion

namespace NebulaNetwork.PacketProcessors.Combat.GroundEnemy;

[RegisterPacketProcessor]
public class DFGUpdateBaseStatusProcessor : PacketProcessor<DFGUpdateBaseStatusPacket>
{
    protected override void ProcessPacket(DFGUpdateBaseStatusPacket packet, NebulaConnection conn)
    {
        var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
        if (factory == null) return;

        if (packet.BaseId >= factory.enemySystem.bases.capacity) return;
        var dFBase = factory.enemySystem.bases.buffer[packet.BaseId];
        if (dFBase == null) return;
        ref var evolveData = ref dFBase.evolve;
        evolveData.threat = packet.Threat;
        if (evolveData.level != packet.Level)
        {
            evolveData.expp = 0;
        }
        evolveData.level = packet.Level;
        evolveData.expl = packet.Expl;
        evolveData.expf = packet.Expf;
    }
}

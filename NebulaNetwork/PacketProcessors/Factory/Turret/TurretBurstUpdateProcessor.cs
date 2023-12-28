#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Ejector;
using NebulaModel.Packets.Factory.Turret;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Ejector;

[RegisterPacketProcessor]
internal class TurretBurstUpdateProcessor : PacketProcessor<TurretBurstUpdatePacket>
{
    protected override void ProcessPacket(TurretBurstUpdatePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.defenseSystem.turrets;
        if (pool != null && packet.TurretIndex != -1)
        {
            UITurretWindow.burstModeIndex = packet.TurretIndex;
        }
    }
}

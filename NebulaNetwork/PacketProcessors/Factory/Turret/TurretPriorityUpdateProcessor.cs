#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Turret;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Turret;

[RegisterPacketProcessor]
internal class TurretPriorityUpdateProcessor : PacketProcessor<TurretPriorityUpdatePacket>
{
    protected override void ProcessPacket(TurretPriorityUpdatePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.defenseSystem.turrets;
        if (pool != null && packet.TurretIndex != -1 && packet.TurretIndex < pool.buffer.Length &&
            pool.buffer[packet.TurretIndex].id != -1)
        {
            pool.buffer[packet.TurretIndex].vsSettings = packet.VSSettings;
        }
    }
}

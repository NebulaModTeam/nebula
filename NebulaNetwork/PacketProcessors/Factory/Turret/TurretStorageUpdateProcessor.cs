#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Ejector;
using NebulaModel.Packets.Factory.Turret;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Ejector;

[RegisterPacketProcessor]
internal class TurretStorageUpdateProcessor : PacketProcessor<TurretStorageUpdatePacket>
{
    protected override void ProcessPacket(TurretStorageUpdatePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.defenseSystem.turrets;
        if (pool == null || packet.TurretIndex == -1 || packet.TurretIndex > pool.buffer.Length ||
            pool.buffer[packet.TurretIndex].id == -1)
        {
            return;
        }


        if (pool.buffer[packet.TurretIndex].itemId != packet.ItemId)
        {
            pool.buffer[packet.TurretIndex].SetNewItem(packet.ItemId, (short)packet.ItemCount, (short)packet.ItemInc);
        }
        else
        {
            pool.buffer[packet.TurretIndex].itemCount = (short)packet.ItemCount;
        }
    }
}

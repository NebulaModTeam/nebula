#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Tank;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Tank;

[RegisterPacketProcessor]
internal class SprayerStorageUpdateProcessor : PacketProcessor<SprayerStorageUpdatePacket>
{
    protected override void ProcessPacket(SprayerStorageUpdatePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.cargoTraffic.spraycoaterPool;
        if (pool == null || packet.SprayerIndex < 0 || packet.SprayerIndex >= pool.Length || pool[packet.SprayerIndex].id == -1)
        {
            return;
        }
        pool[packet.SprayerIndex].incItemId = packet.IncItemId;
        pool[packet.SprayerIndex].incAbility = packet.IncAbility;
        pool[packet.SprayerIndex].incSprayTimes = packet.IncSprayTimes;
        pool[packet.SprayerIndex].incCount = packet.IncCount;
        pool[packet.SprayerIndex].extraIncCount = packet.ExtraIncCount;
    }
}

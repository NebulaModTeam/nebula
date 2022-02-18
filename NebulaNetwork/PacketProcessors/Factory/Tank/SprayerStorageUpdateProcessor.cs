﻿using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Tank;

namespace NebulaNetwork.PacketProcessors.Factory.Tank
{
    [RegisterPacketProcessor]
    internal class SprayerStorageUpdateProcessor : PacketProcessor<SprayerStorageUpdatePacket>
    {
        public override void ProcessPacket(SprayerStorageUpdatePacket packet, NebulaConnection conn)
        {
            SpraycoaterComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.cargoTraffic.spraycoaterPool;
            if (pool != null && packet.SprayerIndex >= 0 && packet.SprayerIndex < pool.Length && pool[packet.SprayerIndex].id != -1)
            {
                pool[packet.SprayerIndex].incItemId = packet.IncItemId;
                pool[packet.SprayerIndex].incAbility = packet.IncAbility;
                pool[packet.SprayerIndex].incSprayTimes = packet.IncSprayTimes;
                pool[packet.SprayerIndex].incCount = packet.IncCount;
                pool[packet.SprayerIndex].extraIncCount = packet.ExtraIncCount;
            }
        }
    }
}

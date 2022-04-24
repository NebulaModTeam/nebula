﻿using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.PowerGenerator;

namespace NebulaNetwork.PacketProcessors.Factory.PowerGenerator
{
    [RegisterPacketProcessor]
    internal class PowerGeneratorProductUpdateProcessor : PacketProcessor<PowerGeneratorProductUpdatePacket>
    {
        public override void ProcessPacket(PowerGeneratorProductUpdatePacket packet, NebulaConnection conn)
        {
            PowerGeneratorComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId).factory?.powerSystem.genPool;
            if (pool != null && packet.PowerGeneratorIndex != -1 && packet.PowerGeneratorIndex < pool.Length && pool[packet.PowerGeneratorIndex].id != -1)
            {
                pool[packet.PowerGeneratorIndex].productCount = packet.ProductCount;
            }
        }
    }
}

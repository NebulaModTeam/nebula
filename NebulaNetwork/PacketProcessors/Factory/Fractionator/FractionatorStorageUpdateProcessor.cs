﻿using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Fractionator;

namespace NebulaNetwork.PacketProcessors.Factory.Fractionator
{
    [RegisterPacketProcessor]
    internal class FractionatorStorageUpdateProcessor : PacketProcessor<FractionatorStorageUpdatePacket>
    {
        public override void ProcessPacket(FractionatorStorageUpdatePacket packet, NebulaConnection conn)
        {
            FractionatorComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId).factory?.factorySystem.fractionatorPool;
            if (pool != null && packet.FractionatorId > 0 && packet.FractionatorId < pool.Length && pool[packet.FractionatorId].id != -1)
            {
                pool[packet.FractionatorId].productOutputCount = packet.ProductOutputCount;
                pool[packet.FractionatorId].fluidOutputCount = packet.FluidOutputCount;
                pool[packet.FractionatorId].fluidOutputInc = packet.FluidOutputInc;
            }
        }
    }
}

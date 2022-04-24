﻿namespace NebulaModel.Packets.Factory.PowerGenerator
{
    public class PowerGeneratorProductUpdatePacket
    {
        public int PowerGeneratorIndex { get; set; }
        public float ProductCount { get; set; }
        public int PlanetId { get; set; }

        public PowerGeneratorProductUpdatePacket() { }
        public PowerGeneratorProductUpdatePacket(in PowerGeneratorComponent powerGenerator, int planetId)
        {
            PowerGeneratorIndex = powerGenerator.id;
            ProductCount = powerGenerator.productCount;
            PlanetId = planetId;
        }
    }
}

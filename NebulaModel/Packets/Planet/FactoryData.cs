using System;

namespace NebulaModel.Packets.Planet
{
    public class FactoryData
    {
        public int PlanetId { get; set; }
        public byte[] BinaryData { get; set; }
        public int[] PlanetIdsWithLogistics { get; set; }

        public FactoryData() { }
        public FactoryData(int id, byte[] data, int[] PlanetIdsWithLogistics)
        {
            this.PlanetId = id;
            this.BinaryData = data;
            this.PlanetIdsWithLogistics = new int[PlanetIdsWithLogistics.Length];
            Array.Copy(PlanetIdsWithLogistics, this.PlanetIdsWithLogistics, PlanetIdsWithLogistics.Length);
        }
    }
}

namespace NebulaModel.Packets.Factory.Assembler
{
    public class AssemblerUpdateStoragePacket
    {
        public int PlanetId { get; set; }
        public int AssemblerIndex { get; set; }
        public int[] Served { get; set; }
        public AssemblerUpdateStoragePacket() { }
        public AssemblerUpdateStoragePacket(int[] served, int planetId, int assemblerIndex)
        {
            PlanetId = planetId;
            AssemblerIndex = assemblerIndex;
            Served = served;
        }
    }
}

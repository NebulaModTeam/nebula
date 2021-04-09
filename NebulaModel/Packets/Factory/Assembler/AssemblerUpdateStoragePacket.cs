namespace NebulaModel.Packets.Factory.Assembler
{
    public class AssemblerUpdateStoragePacket
    {
        public int FactoryIndex { get; set; }
        public int AssemblerIndex { get; set; }
        public int[] Served { get; set; }
        public AssemblerUpdateStoragePacket() { }
        public AssemblerUpdateStoragePacket(int[] served, int factoryIndex, int assemblerIndex)
        {
            FactoryIndex = factoryIndex;
            AssemblerIndex = assemblerIndex;
            Served = served;
        }
    }
}

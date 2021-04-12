namespace NebulaModel.Packets.Factory.Assembler
{
    public class AssemblerUpdateProducesPacket
    {
        public int FactoryIndex { get; set; }
        public int AssemblerIndex { get; set; }
        public int ProducesIndex { get; set; }
        public int ProducesValue { get; set; }

        public AssemblerUpdateProducesPacket() { }
        public AssemblerUpdateProducesPacket(int producesIndex, int producesValue, int factoryIndex, int assemblerIndex)
        {
            FactoryIndex = factoryIndex;
            AssemblerIndex = assemblerIndex;
            ProducesIndex = producesIndex;
            ProducesValue = producesValue;
        }
    }
}

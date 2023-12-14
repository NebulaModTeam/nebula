namespace NebulaModel.Packets.Factory.Assembler;

public class AssemblerUpdateProducesPacket
{
    public AssemblerUpdateProducesPacket() { }

    public AssemblerUpdateProducesPacket(int producesIndex, int producesValue, int planetId, int assemblerIndex)
    {
        PlanetId = planetId;
        AssemblerIndex = assemblerIndex;
        ProducesIndex = producesIndex;
        ProducesValue = producesValue;
    }

    public int PlanetId { get; set; }
    public int AssemblerIndex { get; set; }
    public int ProducesIndex { get; set; }
    public int ProducesValue { get; set; }
}

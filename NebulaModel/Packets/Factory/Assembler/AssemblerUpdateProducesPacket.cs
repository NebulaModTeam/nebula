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

    public int PlanetId { get; }
    public int AssemblerIndex { get; }
    public int ProducesIndex { get; }
    public int ProducesValue { get; }
}

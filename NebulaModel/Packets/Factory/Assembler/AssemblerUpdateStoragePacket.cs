namespace NebulaModel.Packets.Factory.Assembler;

public class AssemblerUpdateStoragePacket
{
    public AssemblerUpdateStoragePacket() { }

    public AssemblerUpdateStoragePacket(int planetId, int assemblerIndex, int[] served, int[] incServed)
    {
        PlanetId = planetId;
        AssemblerIndex = assemblerIndex;
        Served = served;
        IncServed = incServed;
    }

    public int PlanetId { get; }
    public int AssemblerIndex { get; }
    public int[] Served { get; }
    public int[] IncServed { get; }
}

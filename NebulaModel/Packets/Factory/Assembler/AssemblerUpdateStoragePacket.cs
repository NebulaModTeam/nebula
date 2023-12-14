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

    public int PlanetId { get; set; }
    public int AssemblerIndex { get; set; }
    public int[] Served { get; set; }
    public int[] IncServed { get; set; }
}

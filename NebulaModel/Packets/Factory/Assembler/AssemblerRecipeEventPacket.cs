namespace NebulaModel.Packets.Factory.Assembler;

public class AssemblerRecipeEventPacket
{
    public AssemblerRecipeEventPacket() { }

    public AssemblerRecipeEventPacket(int planetId, int assemblerIndex, int recipeId)
    {
        PlanetId = planetId;
        AssemblerIndex = assemblerIndex;
        RecipeId = recipeId;
    }

    public int PlanetId { get; }
    public int AssemblerIndex { get; }
    public int RecipeId { get; }
}

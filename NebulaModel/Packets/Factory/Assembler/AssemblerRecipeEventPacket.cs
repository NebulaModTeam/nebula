namespace NebulaModel.Packets.Factory.Assembler
{
    public class AssemblerRecipeEventPacket
    {
        public int FactoryIndex { get; set; }
        public int AssemblerIndex { get; set; }
        public int RecipeId { get; set; }

        public AssemblerRecipeEventPacket() { }

        public AssemblerRecipeEventPacket(int factoryIndex, int assemblerIndex, int recipeId)
        {
            FactoryIndex = factoryIndex;
            AssemblerIndex = assemblerIndex;
            RecipeId = recipeId;
        }
    }
}

namespace NebulaModel.Packets.Factory
{
    public class PasteEntitySettingUpdate
    {
        public int EntityId { get; set; }
        public int RecipeId { get; set; }
        public bool ResearchMode { get; set; }
        public int FilterId { get; set; }
        public int OrbitId { get; set; }
        public int Mode { get; set; }
        public ERecipeType RecipeType { get; set; }
        public EntitySettingType Type { get; set; }

        public PasteEntitySettingUpdate() { }

        public PasteEntitySettingUpdate(int entityId, EntitySettingDesc clipboard)
        {
            EntityId = entityId;
            RecipeId = clipboard.recipeId;
            ResearchMode = clipboard.researchMode;
            FilterId = clipboard.filterId;
            OrbitId = clipboard.orbitId;
            Mode = clipboard.mode;
            RecipeType = clipboard.recipeType;
            Type = clipboard.type;
        }

        public EntitySettingDesc GetEntitySettings()
        {
            EntitySettingDesc result = new EntitySettingDesc();
            result.recipeType = RecipeType;
            result.recipeId = RecipeId;
            result.filterId = FilterId;
            result.researchMode = ResearchMode;
            result.orbitId = OrbitId;
            result.mode = Mode;
            result.recipeType = RecipeType;
            result.type = Type;
            return result;
        }
    }
}

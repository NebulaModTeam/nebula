namespace NebulaModel.Packets.Factory;

public class PasteBuildingSettingUpdate
{
    public PasteBuildingSettingUpdate() { }

    public PasteBuildingSettingUpdate(int objectId, BuildingParameters clipboard, int planetId)
    {
        ObjectId = objectId;
        Type = clipboard.type;
        ItemId = clipboard.itemId;
        ModelIndex = clipboard.modelIndex;
        Yaw = clipboard.yaw;
        RecipeType = clipboard.recipeType;
        RecipeId = clipboard.recipeId;
        FilterId = clipboard.filterId;
        Mode0 = clipboard.mode0;
        Mode1 = clipboard.mode1;
        Mode2 = clipboard.mode2;
        Mode3 = clipboard.mode3;
        Parameters = clipboard.parameters;
        InserterItemIds = clipboard.inserterItemIds;
        InserterLengths = clipboard.inserterLengths;
        InserterFilters = clipboard.inserterFilters;
        PlanetId = planetId;
    }

    public int ObjectId { get; set; }
    public BuildingType Type { get; set; }
    public int ItemId { get; set; }
    public int ModelIndex { get; set; }
    public float Yaw { get; set; }
    public ERecipeType RecipeType { get; set; }
    public int RecipeId { get; set; }
    public int FilterId { get; set; }
    public int Mode0 { get; set; }
    public int Mode1 { get; set; }
    public int Mode2 { get; set; }
    public int Mode3 { get; set; }
    public int[] Parameters { get; set; }
    public int[] InserterItemIds { get; set; }
    public int[] InserterLengths { get; set; }
    public int[] InserterFilters { get; set; }
    public int PlanetId { get; set; }

    public BuildingParameters GetBuildingSettings()
    {
        var result = new BuildingParameters
        {
            type = Type,
            itemId = ItemId,
            modelIndex = ModelIndex,
            yaw = Yaw,
            recipeType = RecipeType,
            recipeId = RecipeId,
            filterId = FilterId,
            mode0 = Mode0,
            mode1 = Mode1,
            mode2 = Mode2,
            mode3 = Mode3,
            parameters = Parameters,
            inserterItemIds = InserterItemIds,
            inserterLengths = InserterLengths,
            inserterFilters = InserterFilters
        };
        return result;
    }
}

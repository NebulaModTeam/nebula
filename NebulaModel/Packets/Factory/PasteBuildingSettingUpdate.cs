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
    private BuildingType Type { get; set; }
    private int ItemId { get; set; }
    private int ModelIndex { get; set; }
    private float Yaw { get; set; }
    private ERecipeType RecipeType { get; set; }
    private int RecipeId { get; set; }
    private int FilterId { get; set; }
    private int Mode0 { get; set; }
    private int Mode1 { get; set; }
    private int Mode2 { get; set; }
    private int Mode3 { get; set; }
    private int[] Parameters { get; set; }
    private int[] InserterItemIds { get; set; }
    private int[] InserterLengths { get; set; }
    private int[] InserterFilters { get; set; }
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

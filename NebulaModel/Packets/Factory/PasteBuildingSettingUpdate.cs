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

    public int ObjectId { get; }
    private BuildingType Type { get; }
    private int ItemId { get; }
    private int ModelIndex { get; }
    private float Yaw { get; }
    private ERecipeType RecipeType { get; }
    private int RecipeId { get; }
    private int FilterId { get; }
    private int Mode0 { get; }
    private int Mode1 { get; }
    private int Mode2 { get; }
    private int Mode3 { get; }
    private int[] Parameters { get; }
    private int[] InserterItemIds { get; }
    private int[] InserterLengths { get; }
    private int[] InserterFilters { get; }
    public int PlanetId { get; }

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

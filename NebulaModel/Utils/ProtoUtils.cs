
namespace NebulaModel.Utils
{
    public static class ProtoUtils
    {
        public static string GetSignalDisplayName(int signalId)
        {
            if (signalId < 1000)
            {
                return $"signal-{signalId}";
            }
            if (signalId < 20000)
            {
                ItemProto proto = LDB.items.Select(signalId);
                return proto.name;
            }
            if (signalId < 40000)
            {
                RecipeProto proto = LDB.recipes.Select(signalId - 20000);
                return proto.name;
            }
            else
            {
                TechProto proto = LDB.techs.Select(signalId - 40000);
                return proto.name;
            }
        }
    }
}
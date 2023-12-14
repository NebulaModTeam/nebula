namespace NebulaModel.Utils;

public static class ProtoUtils
{
    public static string GetSignalDisplayName(int signalId)
    {
        if (signalId < 1000)
        {
            var signal = LDB.signals.Select(signalId);
            return signal.name;
            //return $"signal-{signalId}";
        }
        if (signalId < 20000)
        {
            var proto = LDB.items.Select(signalId);
            return proto.name;
        }
        if (signalId < 40000)
        {
            var proto = LDB.recipes.Select(signalId - 20000);
            return proto.name;
        }
        else
        {
            var proto = LDB.techs.Select(signalId - 40000);
            return proto.name;
        }
    }
}

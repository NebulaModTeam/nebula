namespace NebulaModel.Utils;

public static class ProtoUtils
{
    public static string GetSignalDisplayName(int signalId)
    {
        switch (signalId)
        {
            case < 1000:
                {
                    var signal = LDB.signals.Select(signalId);
                    return signal.name;
                    //return $"signal-{signalId}";
                }
            case < 20000:
                {
                    var proto = LDB.items.Select(signalId);
                    return proto.name;
                }
            case < 40000:
                {
                    var proto = LDB.recipes.Select(signalId - 20000);
                    return proto.name;
                }
            default:
                {
                    var proto = LDB.techs.Select(signalId - 40000);
                    return proto.name;
                }
        }
    }
}

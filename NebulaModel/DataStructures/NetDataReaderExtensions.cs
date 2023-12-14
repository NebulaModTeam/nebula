#region

using NebulaAPI.DataStructures;
using NebulaAPI.Interfaces;

#endregion

namespace NebulaModel.DataStructures;

public static class NetDataReaderExtensions
{
    public static Float3 GetFloat3(this INetDataReader reader)
    {
        var value = new Float3();
        value.Deserialize(reader);
        return value;
    }

    public static Float4 GetFloat4(this INetDataReader reader)
    {
        var value = new Float4();
        value.Deserialize(reader);
        return value;
    }

    public static Double3 GetDouble3(this INetDataReader reader)
    {
        var value = new Double3();
        value.Deserialize(reader);
        return value;
    }
}

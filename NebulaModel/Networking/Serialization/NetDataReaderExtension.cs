
using NebulaAPI.Interfaces;

namespace NebulaModel.Networking.Serialization;

public static class NetDataReaderExtension
{
    /// <remarks>
    /// https://github.com/RevenantX/LiteNetLib/commit/2f9eefcbcec9d3f8243d2d8d5e757d2133aafcbe
    /// This was removed in above commit for what seems to be performance reasons. Currently only used by `NebulaModel.DataStructures.PlayerData`
    /// @TODO: Investigate if it proves necessary.
    /// </remarks>
    public static T Get<T>(this NetDataReader reader) where T : class, INetSerializable, new()
    {
        var obj = new T();
        obj.Deserialize(reader);
        return obj;
    }
}

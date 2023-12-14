#region

using UnityEngine;

#endregion

namespace NebulaAPI.DataStructures;

public static class DataStructureExtensions
{
    public static Vector3 ToVector3(this Float3 value)
    {
        return new Vector3(value.x, value.y, value.z);
    }

    public static VectorLF3 ToVectorLF3(this Double3 value)
    {
        return new VectorLF3(value.x, value.y, value.z);
    }

    public static Float3 ToFloat3(this Vector3 value)
    {
        return new Float3(value.x, value.y, value.z);
    }

    public static Quaternion ToQuaternion(this Float4 value)
    {
        return new Quaternion(value.x, value.y, value.z, value.w);
    }

    public static Float4 ToFloat4(this Quaternion value)
    {
        return new Float4(value.x, value.y, value.z, value.w);
    }
}

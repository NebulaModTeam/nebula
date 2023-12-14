#region

using NebulaAPI.Interfaces;
using NebulaAPI.Packets;
using UnityEngine;

#endregion

namespace NebulaAPI.DataStructures;

[RegisterNestedType]
public struct Float4 : INetSerializable
{
    public float x;
    public float y;
    public float z;
    public float w;

    public Float4(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public Float4(Quaternion value)
    {
        x = value.x;
        y = value.y;
        z = value.z;
        w = value.w;
    }

    public Color ToColor()
    {
        return new Color(x, y, z, w);
    }

    public Color32 ToColor32()
    {
        return new Color32((byte)x, (byte)y, (byte)z, (byte)w);
    }

    public static Color32[] ToColor32(Float4[] float4s)
    {
        var color32s = new Color32[float4s.Length];
        for (var i = 0; i < float4s.Length; i++)
        {
            color32s[i] = float4s[i].ToColor32();
        }
        return color32s;
    }

    public static Float4 ToFloat4(Color32 color32)
    {
        return new Float4(color32.r, color32.g, color32.b, color32.a);
    }

    public static Float4[] ToFloat4(Color32[] color32s)
    {
        var float4s = new Float4[color32s.Length];
        for (var i = 0; i < color32s.Length; i++)
        {
            float4s[i] = new Float4(color32s[i].r, color32s[i].g, color32s[i].b, color32s[i].a);
        }
        return float4s;
    }

    public void Serialize(INetDataWriter writer)
    {
        writer.Put(x);
        writer.Put(y);
        writer.Put(z);
        writer.Put(w);
    }

    public void Deserialize(INetDataReader reader)
    {
        x = reader.GetFloat();
        y = reader.GetFloat();
        z = reader.GetFloat();
        w = reader.GetFloat();
    }


    public override string ToString()
    {
        return $"x: {x}, y: {y}, z: {z}, w: {w}";
    }
}

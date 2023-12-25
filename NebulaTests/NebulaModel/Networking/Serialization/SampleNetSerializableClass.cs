// #pragma once
// #ifndef SampleNetSerializable.cs_H_
// #define SampleNetSerializable.cs_H_
// 
// #endif

using NebulaAPI.Interfaces;

namespace NebulaTests.NebulaModel.Networking.Serialization;

public class SampleNetSerializableClass : INetSerializable, IEquatable<SampleNetSerializableClass>
{
    public int value;

    public void Serialize(INetDataWriter writer)
    {
        writer.Put(value);
    }

    public void Deserialize(INetDataReader reader)
    {
        value = reader.GetInt();
    }

    public bool Equals(SampleNetSerializableClass? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return value == other.value;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((SampleNetSerializableClass)obj);
    }

    public override int GetHashCode()
    {
        return value;
    }

    public static bool operator ==(SampleNetSerializableClass lhs, SampleNetSerializableClass rhs)
    {
        if (lhs is null)
        {
            if (rhs is null)
            {
                return true;
            }
            // Only the left side is null.
            return false;
        }

        // Equals handles case of null on right side.
        return lhs.Equals(rhs);
    }

    public static bool operator !=(SampleNetSerializableClass lhs, SampleNetSerializableClass rhs) => !(lhs == rhs);
}
using NebulaAPI.Interfaces;

namespace NebulaTests.NebulaModel.Networking.Serialization;

public struct SampleNetSerializableStruct : INetSerializable, IEquatable<SampleNetSerializableStruct>
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

    public bool Equals(SampleNetSerializableStruct other)
    {
        return value == other.value;
    }

    public override bool Equals(object? obj)
    {
        return obj is SampleNetSerializableStruct other && Equals(other);
    }

    public override int GetHashCode()
    {
        return value;
    }

    public static bool operator ==(SampleNetSerializableStruct lhs, SampleNetSerializableStruct rhs)
    {
        // Equals handles case of null on right side.
        return lhs.Equals(rhs);
    }

    public static bool operator !=(SampleNetSerializableStruct lhs, SampleNetSerializableStruct rhs) => !(lhs == rhs);
}

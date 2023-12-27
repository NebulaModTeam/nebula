namespace NebulaTests.NebulaModel.Networking.Serialization;

public class SampleCustomClass : IEquatable<SampleCustomClass>
{
    public int Value { get; set; }

    public bool Equals(SampleCustomClass? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Value == other.Value;
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

        return Equals((SampleCustomClass)obj);
    }

    public override int GetHashCode()
    {
        return Value;
    }

    public static bool operator ==(SampleCustomClass lhs, SampleCustomClass rhs)
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

    public static bool operator !=(SampleCustomClass lhs, SampleCustomClass rhs) => !(lhs == rhs);
}
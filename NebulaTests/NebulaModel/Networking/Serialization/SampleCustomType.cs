namespace NebulaTests.NebulaModel.Networking.Serialization;

public class SampleCustomType : IEquatable<SampleCustomType>
{
    public int Value { get; set; }

    public bool Equals(SampleCustomType? other)
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

        return Equals((SampleCustomType)obj);
    }

    public override int GetHashCode()
    {
        return Value;
    }

    public static bool operator ==(SampleCustomType lhs, SampleCustomType rhs)
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

    public static bool operator !=(SampleCustomType lhs, SampleCustomType rhs) => !(lhs == rhs);
}
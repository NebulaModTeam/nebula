namespace NebulaTests.Functional;

internal enum BandwidthOrdinal
{
    None = 0,
    Kilo = 1,
    Mega = 2,
    Giga = 3,
    Tera = 4
}

internal struct Bandwidth(BandwidthOrdinal ordinal, double speed, bool isBitsPerSecond = true)
{
    public BandwidthOrdinal Ordinal { get; } = ordinal;
    public double Speed { get; } = speed;

    public bool IsBitsPerSecond { get; } = isBitsPerSecond;

    public static bool operator ==(Bandwidth lhs, Bandwidth rhs)
    {
        return lhs.Ordinal == rhs.Ordinal && Math.Abs(lhs.Speed - rhs.Speed) < 0.01;
    }

    public static bool operator !=(Bandwidth lhs, Bandwidth rhs) => !(lhs == rhs);

    public static bool operator >(Bandwidth lhs, Bandwidth rhs)
    {
        if (lhs.Ordinal > rhs.Ordinal)
            return true;
        return lhs.Ordinal == rhs.Ordinal && lhs.Speed > rhs.Speed;
    }

    public static bool operator <(Bandwidth lhs, Bandwidth rhs) => !(lhs > rhs);

    public static bool operator >=(Bandwidth lhs, Bandwidth rhs) => lhs > rhs || lhs == rhs;

    public static bool operator <=(Bandwidth lhs, Bandwidth rhs) => lhs < rhs || lhs == rhs;
}

internal static class BandwidthFormatter
{
    /// <summary>
    /// Formats a number of bytes to it's relevant ordinal based on it's amount.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="formatAsBits">If true the speed will be formatted to *bps instead of *B/s</param>
    /// <returns></returns>
    internal static string FormatString(double bytes, bool formatAsBits = true)
    {
        var ordinalStrings = new[] { "", "K", "M", "G", "T" };

        var ordinal = BandwidthOrdinal.None;

        while (bytes > 1000)
        {
            bytes /= 1000;
            ordinal++;
        }

        var suffix = "B";
        if (!formatAsBits)
            return $"{Math.Round(bytes, 2, MidpointRounding.AwayFromZero)} {ordinalStrings[(int)ordinal]}{suffix}";

        bytes *= 8;
        suffix = "bps";

        return $"{Math.Round(bytes, 2, MidpointRounding.AwayFromZero)} {ordinalStrings[(int)ordinal]}{suffix}";
    }

    /// <summary>
    /// Returns the ordinal & the speed number in that ordinal.
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="formatAsBits">If true the speed will be formatted to *bps instead of *B/s</param>
    /// <returns></returns>
    internal static Bandwidth ToBandwidth(double bytes, bool formatAsBits = true)
    {
        var ordinal = BandwidthOrdinal.None;

        while (bytes > 1000)
        {
            bytes /= 1000;
            ordinal++;
        }

        if (formatAsBits)
        {
            bytes *= 8;
        }

        return new Bandwidth(ordinal, Math.Round(bytes, 2, MidpointRounding.AwayFromZero));
    }
}

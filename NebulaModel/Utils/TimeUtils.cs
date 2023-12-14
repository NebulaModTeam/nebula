#region

using System;

#endregion

namespace NebulaModel.Utils;

public class TimeUtils
{
    private static readonly DateTime UNIX_EPOCH = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static long CurrentUnixTimestampMilliseconds()
    {
        return (long)(DateTime.UtcNow - UNIX_EPOCH).TotalMilliseconds;
    }
}

using System;

namespace NebulaModel.Utils
{
    public class TimeUtils
    {
        static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentUnixTimestampMilliseconds()
        {
            return (long)(DateTime.UtcNow - UNIX_EPOCH).TotalMilliseconds;
        }
    }
}

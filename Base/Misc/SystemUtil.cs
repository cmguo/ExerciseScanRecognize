using System;

namespace Base.Misc
{
    public static class SystemUtil
    {
        private static readonly DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static long Timestamp(this DateTime date)
        {
            TimeSpan diff = date.ToUniversalTime() - origin;
            return (long) diff.TotalSeconds;
        }

        public static DateTime DateTimeFromTimestamp(long timestamp)
        {
            return origin.AddSeconds(timestamp).ToLocalTime();
        }
    }
}

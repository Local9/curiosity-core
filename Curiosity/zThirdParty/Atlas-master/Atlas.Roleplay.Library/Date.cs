using System;

namespace Atlas.Roleplay.Library
{
    public class Date
    {
        public static long Timestamp => DateTime.UtcNow.Ticks / 10000;

        public static long TimestampToTicks(long timestamp)
        {
            return timestamp * 10000;
        }
    }
}
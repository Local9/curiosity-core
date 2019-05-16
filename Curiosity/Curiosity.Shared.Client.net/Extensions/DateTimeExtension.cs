using System;

namespace Curiosity.Shared.Client.net.Extensions
{
    public static class DateTimeExtension
    {
        public static double GetUnixEpoch(this DateTime dateTime)
        {
            var unixTime = dateTime.ToUniversalTime() -
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return unixTime.TotalSeconds;
        }
    }
}

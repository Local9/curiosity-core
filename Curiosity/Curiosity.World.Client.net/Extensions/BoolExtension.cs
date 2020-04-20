using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.GameWorld.Client.net.Extensions
{
    static class BoolExtension
    {
        public static bool IsBetween<T>(this T item, T start, T end)
        {
            return Comparer<T>.Default.Compare(item, start) >= 0
                && Comparer<T>.Default.Compare(item, end) <= 0;
        }

        public static bool IsTimeOfDayBetween(this TimeSpan time, TimeSpan startTime, TimeSpan endTime)
        {
            if (endTime == startTime)
            {
                return true;
            }
            else if (endTime < startTime)
            {
                return time <= endTime ||
                    time >= startTime;
            }
            else
            {
                return time >= startTime &&
                    time <= endTime;
            }
        }
    }
}

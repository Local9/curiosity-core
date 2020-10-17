using System;
using System.Collections.Generic;

namespace Curiosity.Global.Shared.Utils
{
    public static class Extensions
    {
        public static bool Bool(this Random random, float propability) => random.NextDouble() < propability;

        public static T Random<T>(this T[] list)
        {
            int index = Utility.RANDOM.Next(list.Length - 1);
            return list[index];
        }

        public static T Random<T>(this List<T> list)
        {
            int index = Utility.RANDOM.Next(list.Count);
            return list[index];
        }
    }
}

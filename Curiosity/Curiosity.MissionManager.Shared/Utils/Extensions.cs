using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Systems.Library.Utils
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

        public static IEnumerable<TValue> RandomValues<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            Random rand = new Random();
            List<TValue> values = Enumerable.ToList(dict.Values);
            int size = dict.Count;
            while (true)
            {
                yield return values[rand.Next(size)];
            }
        }

        public static IEnumerable<TValue> UniqueRandomValues<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            Random rand = new Random();
            Dictionary<TKey, TValue> values = new Dictionary<TKey, TValue>(dict);
            while (values.Count > 0)
            {
                TKey randomKey = values.Keys.ElementAt(rand.Next(0, values.Count));  // hat tip @yshuditelu 
                TValue randomValue = values[randomKey];
                values.Remove(randomKey);
                yield return randomValue;
            }
        }

        public static IEnumerable<TValue> UniqueShuffledRandomValues<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            // Put the values in random order
            Random rand = new Random();
            LinkedList<TValue> values = new LinkedList<TValue>(from v in dict.Values
                                                               orderby rand.Next()
                                                               select v);
            // Remove the values one at a time
            while (values.Count > 0)
            {
                yield return values.Last.Value;
                values.RemoveLast();
            }
        }
    }
}

using Curiosity.Systems.Library.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Curiosity.Systems.Library.Utils
{
    public static class Extensions
    {
        public static T[] Append<T>(this T[] array, T item)
        {
            if (array == null)
            {
                return new T[] { item };
            }
            T[] result = new T[array.Length + 1];
            array.CopyTo(result, 0);
            result[array.Length] = item;
            return result;
        }

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
        
        /// <summary>
         /// Will get the string value for a given enums value, this will
         /// only work if you assign the StringValue attribute to
         /// the items in your enum.
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
        public static string GetStringValue(this Enum value)
        {
            // Get the type
            Type type = value.GetType();

            // Get fieldinfo for this type
            FieldInfo fieldInfo = type.GetField(value.ToString());

            // Get the stringvalue attributes
            StringValueAttribute[] attribs = fieldInfo.GetCustomAttributes(
                typeof(StringValueAttribute), false) as StringValueAttribute[];

            // Return the first if there was a match.
            return attribs.Length > 0 ? attribs[0].StringValue : null;
        }
    }
}

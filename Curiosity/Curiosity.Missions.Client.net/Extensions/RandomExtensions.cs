using System;

namespace Curiosity.Missions.Client.Extensions
{
    static class RandomExtensions
    {
        public static void Shuffle<T>(this Random rng, T[] array)
        {
            int length = (int)array.Length;
            while (length > 1)
            {
                int num = length;
                length = num - 1;
                int num1 = rng.Next(num);
                T t = array[length];
                array[length] = array[num1];
                array[num1] = t;
            }
        }
    }
}

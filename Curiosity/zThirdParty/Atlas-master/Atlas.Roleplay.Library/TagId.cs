using System;
using System.Text;

namespace Atlas.Roleplay.Library
{
    public class TagId
    {
        public static char[] Alphabet { get; } = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        public static char[] Numbers { get; } = "123456789".ToCharArray();

        public static string Generate(int length)
        {
            var builder = new StringBuilder();
            var random = new Random();

            for (var i = 0; i < length; i++)
            {
                builder.Append(random.Next(1) == 0
                    ? Alphabet[random.Next(Alphabet.Length)]
                    : Numbers[random.Next(Numbers.Length)]);
            }

            return builder.ToString();
        }
    }
}
using System;
using System.Linq;

namespace Curiosity.Systems.Library
{
    public static class Extensions
    {
        public static string ToTitleCase(this string value)
        {
            return String.Join(" ", value.Trim().Split(' ').Select(i => i.Substring(0, 1).ToUpper() + i.Substring(1).ToLower()).ToArray());
        }
    }
}

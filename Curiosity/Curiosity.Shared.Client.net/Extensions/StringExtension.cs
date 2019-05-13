using System;
using System.Linq;

namespace Curiosity.Shared.Client.net.Extensions
{
    public static class StringExtension
    {
        public static string ToTitleCase(this string value)
        {
            return String.Join(" ", value.Split(' ').Select(i => i.Substring(0, 1).ToUpper() + i.Substring(1).ToLower()).ToArray());
        }
    }
}

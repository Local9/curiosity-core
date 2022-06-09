﻿using System.Text;

namespace Curiosity.Framework.Shared.Extensions
{
    public static class Base64Extensions
    {
        public static string ToBase64(this string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        public static string FromBase64(this string serialized)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(serialized));
        }
    }
}

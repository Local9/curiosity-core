using System;

namespace Curiosity.Shared.Client.net.Extensions
{
    public static class EnumExtension
    {
        //checks to see if an enumerated value contains a type
        public static bool Has<T>(this Enum type, T value)
        {
            try
            {
                return (((int)(object)type & (int)(object)value) == (int)(object)value);
            }
            catch
            {
                return false;
            }
        }

        public static T ToEnum<T>(this string value, T defaultValue) where T : struct
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            T result;
            return Enum.TryParse<T>(value, true, out result) ? result : defaultValue;
        }
    }
}

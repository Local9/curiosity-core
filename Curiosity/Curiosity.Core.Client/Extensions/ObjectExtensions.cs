﻿namespace Curiosity.Core.Client.Extensions
{
    static class ObjectExtensions
    {
        public static string ToProperCase(this string the_string)
        {
            // If there are 0 or 1 characters, just return the string.
            if (the_string == null) return the_string;
            if (the_string.Length < 2) return the_string.ToUpper();

            // Start with the first character.
            string result = the_string.Substring(0, 1).ToUpper();

            // Add the remaining characters.
            for (int i = 1; i < the_string.Length; i++)
            {
                if (char.IsUpper(the_string[i])) result += " ";
                result += the_string[i];
            }

            return result;
        }
        public static bool ToBoolean(this object o)
        {
            switch ($"{o}".ToLower())
            {
                case "true":
                    return true;
                case "t":
                    return true;
                case "1":
                    return true;
                case "0":
                    return false;
                case "false":
                    return false;
                case "f":
                    return false;
                default:
                    throw new InvalidCastException("You can't cast that value to a bool!");
            }
        }

        public static Int32 ToInt32(this object o)
        {
            return Convert.ToInt32($"{o}");
        }
        public static Int64 ToInt64(this object o)
        {
            return Convert.ToInt64($"{o}");
        }

        public static int ToInt(this object o)
        {
            return int.Parse($"{o}");
        }

        public static long ToLong(this object o)
        {
            return long.Parse($"{o}");
        }

        public static DateTime ToDateTime(this object o)
        {
            return DateTime.Parse($"{o}");
        }

        public static float ToFloat(this object o)
        {
            return float.Parse($"{o}");
        }

        public static float ToHeading(this Vector3 v)
        {
            float X = v.X;
            float Y = v.Y;
            return (float)((System.Math.Atan2(X, -Y) + System.Math.PI) * (180.0 / System.Math.PI));
        }
    }
}
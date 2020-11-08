using System;

namespace Curiosity.MissionManager.Server.Extensions
{
    static class ObjectExtension
    {
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
    }
}

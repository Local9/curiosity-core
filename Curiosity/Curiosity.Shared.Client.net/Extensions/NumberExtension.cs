namespace Curiosity.Shared.Client.net.Extensions
{
    public static class NumberExtension
    {
        public static bool IsBetween(this int value, int minimum, int maximum)
        {
            return value >= minimum && value <= maximum;
        }

        public static bool IsBetween(this float value, float minimum, float maximum)
        {
            return value >= minimum && value <= maximum;
        }
    }
}

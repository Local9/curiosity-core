namespace Curiosity.Shared.Client.net.Extensions
{
    public static class EnumExtension
    {
        //checks to see if an enumerated value contains a type
        public static bool Has<T>(this System.Enum type, T value)
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
    }
}

namespace Curiosity.Framework.Shared
{
    internal class Common
    {
        public static Random RANDOM = new Random();

        public static async Task MoveToMainThread() => await BaseScript.Delay(0);

        // Get Random Long with max number
        public static long GetRandomLong(long max)
        {
            var bytes = new byte[8];
            RANDOM.NextBytes(bytes);
            var longRand = BitConverter.ToInt64(bytes, 0);
            return (Math.Abs(longRand % max));
        }

        public static float Normalize(float value, float min, float max)
        {
            return (value - min) / (max - min);
        }

        public static float Denormalize(float normalized, float min, float max)
        {
            return (normalized * (max - min) + min);
        }

        // Denormalize a value with min and max

    }
}

using System;

namespace Curiosity.Shared.Client.net.Extensions
{
    public static class BoolExtension
    {
        public static bool NextBool(this Random r, int truePercentage = 50)
        {
            return r.NextDouble() < truePercentage / 100.0;
        }
    }
}

using CitizenFX.Core;
using System;

namespace Curiosity.Shared.Client.net.Extensions
{
    public static class FloatExtension
    {
        public static float[] ToArray(this Vector3 vector)
        {
            try
            {
                return new float[] { vector.X, vector.Y, vector.Z };
            }
            catch (Exception ex)
            {
                Log.Error($"ToArray exception: {ex.Data}");
            }
            return null;
        }
    }
}

using CitizenFX.Core;
using System;

namespace Curiosity.Shared.Client.net.Extensions
{
    public static class VectorExtension
    {
        public static Vector3 ToVector3(this float[] xyzArray)
        {
            try
            {
                return new Vector3(xyzArray[0], xyzArray[1], xyzArray[2]);
            }
            catch (Exception e)
            {
                Log.Error($"ToVector3 exception: {e}");
            }
            return Vector3.Zero;
        }
    }
}

using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.CasinoSystems.Client.Extensions
{
    static class VectorExtension
    {
        public static float Distance(this Vector3 position, Vector3 target, bool useZ = false)
        {
            return API.GetDistanceBetweenCoords(position.X, position.Y, position.Z, target.X, target.Y, target.Z, useZ);
        }
    }
}

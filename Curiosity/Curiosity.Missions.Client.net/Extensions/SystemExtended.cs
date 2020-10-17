using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Missions.Client.Extensions
{
    static class SystemExtended
    {
        public static float VDist(this Vector3 v, Vector3 to)
        {
            return Function.Call<float>((Hash)3046839180162419877L, new InputArgument[] { v.X, v.Y, v.Z, to.X, to.Y, to.Z });
        }
    }
}

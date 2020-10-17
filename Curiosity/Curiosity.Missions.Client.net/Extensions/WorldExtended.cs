using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Missions.Client.Extensions
{
    static class WorldExtended
    {
        public static int CreateParticleEffectAtCoord(Vector3 coord, string name)
        {
            Function.Call((Hash)7798175403732277905L, new InputArgument[] { "core" });
            return API.StartParticleFxLoopedAtCoord(name, coord.X, coord.Y, coord.Z, 0, 0, 0, 1f, false, false, false, false);
        }
    }
}

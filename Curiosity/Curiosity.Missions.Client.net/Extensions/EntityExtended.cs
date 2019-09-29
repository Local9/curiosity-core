using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Missions.Client.net.Extensions
{
    static class EntityExtended
    {
        static public void Fade(this Entity entity, bool state)
        {
            InputArgument[] handle = new InputArgument[] { entity.Handle, default(InputArgument) };
            handle[1] = (state ? 1 : 0);
            Function.Call((Hash)2255972746681902637L, handle);
        }

        public static bool HasClearLineOfSight(this Entity entity, Entity target, float visionDistance)
        {
            return (!Function.Call<bool>((Hash)173335856089985402L, new InputArgument[] { entity.Handle, target.Handle }) ? false : entity.Position.VDist(target.Position) < visionDistance);
        }

        public static bool IsPlayingAnim(this Entity entity, string animSet, string animName)
        {
            return Function.Call<bool>((Hash)2237014829242392265L, new InputArgument[] { entity.Handle, animSet, animName, 3 });
        }
    }
}

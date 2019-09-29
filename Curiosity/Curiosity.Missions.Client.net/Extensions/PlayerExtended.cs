using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Missions.Client.net.Extensions
{
    static class PlayerExtended
    {
        public static void IgnoreLowPriorityShockingEvents(this Player player, bool toggle)
        {
            InputArgument[] handle = new InputArgument[] { player.Handle, default(InputArgument) };
            handle[1] = (toggle ? 1 : 0);
            Function.Call((Hash)6442811240944981760L, handle);
        }
    }
}

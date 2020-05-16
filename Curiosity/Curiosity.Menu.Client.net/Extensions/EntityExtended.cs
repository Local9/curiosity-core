using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Menus.Client.net.Extensions
{
    static class EntityExtended
    {
        static public void Fade(this Entity entity, bool state)
        {
            InputArgument[] handle = new InputArgument[] { entity.Handle, default(InputArgument) };
            handle[1] = (state ? 1 : 0);
            Function.Call((Hash)2255972746681902637L, handle);
        }
    }
}

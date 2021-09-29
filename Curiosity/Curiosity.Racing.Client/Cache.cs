using CitizenFX.Core;

using static CitizenFX.Core.Native.API;

namespace Curiosity.Racing.Client
{
    public class Cache
    {
        public static Player Player => new Player(GetPlayerPed(PlayerId()));
    }
}
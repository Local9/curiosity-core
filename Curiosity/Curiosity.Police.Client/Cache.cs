using CitizenFX.Core;

using static CitizenFX.Core.Native.API;

namespace Curiosity.Police.Client
{
    public class Cache
    {
        public static Player Player => new Player(GetPlayerPed(PlayerId()));
    }
}
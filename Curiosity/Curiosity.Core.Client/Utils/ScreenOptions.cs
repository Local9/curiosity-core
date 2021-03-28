using CitizenFX.Core;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Utils
{
    public static class ScreenOptions
    {
        public static async Task ScreenFadeOut(int millis)
        {
            ScreenFadeOut(millis);
            while(IsScreenFadingOut())
            {
                await BaseScript.Delay(10);
            }
        }

        public static async Task ScreenFadeIn(int millis)
        {
            ScreenFadeIn(millis);
            while (IsScreenFadingIn())
            {
                await BaseScript.Delay(10);
            }
        }
    }
}

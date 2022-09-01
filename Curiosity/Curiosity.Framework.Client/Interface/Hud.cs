using CitizenFX.Core.UI;
using ScaleformUI;

namespace Curiosity.Framework.Client.Interface
{
    internal class Hud
    {
        public static MenuPool MenuPool;
        
        public Hud()
        {
            MenuPool = new MenuPool();
        }

        internal static async Task FadeOut(int duration)
        {
            Screen.Fading.FadeOut(duration);
            while (Screen.Fading.IsFadingOut)
            {
                await BaseScript.Delay(0);
            }
        }

        internal static async Task FadeIn(int duration)
        {
            Screen.Fading.FadeIn(duration);
            while (Screen.Fading.IsFadingIn)
            {
                await BaseScript.Delay(0);
            }
        }
    }
}

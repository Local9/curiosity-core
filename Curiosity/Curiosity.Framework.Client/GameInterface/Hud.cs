using CitizenFX.Core.UI;
using ScaleformUI;

namespace Curiosity.Framework.Client.GameInterface
{
    public class Hud
    {
        public static MenuPool MenuPool;
        
        public Hud()
        {
            MenuPool = new MenuPool();
        }

        internal static async Task FadeOut(int duration, bool giveControlHalfway = false)
        {
            Screen.Fading.FadeOut(duration);

            int ticks = (int)(duration / 2);

            while (Screen.Fading.IsFadingOut)
            {
                await BaseScript.Delay(0);

                if (giveControlHalfway)
                {
                    --ticks;

                    if (ticks <= 0)
                        break;
                }
            }
        }

        internal static async Task FadeIn(int duration, bool giveControlHalfway = false)
        {
            Screen.Fading.FadeIn(duration);

            int ticks = (int)(duration / 2);

            while (Screen.Fading.IsFadingIn)
            {
                await BaseScript.Delay(0);

                if (giveControlHalfway)
                {
                    --ticks;

                    if (ticks <= 0)
                        break;
                }
            }
        }
    }
}

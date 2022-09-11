using CitizenFX.Core.UI;
using Curiosity.Framework.Shared.Enums;
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

        internal static void ShowNotificationSuccess(string message, bool blink = true, bool saveToBrief = true)
        {
            ShowNotification(message, blink, saveToBrief, eHudColor.HUD_COLOUR_GREENLIGHT);
        }

        internal static void ShowNotification(string message, bool blink = true, bool saveToBrief = true, eHudColor bgColor = eHudColor.HUD_COLOUR_BLACK)
        {
            string[] strings = Screen.StringToArray(message);
            API.SetNotificationTextEntry("CELL_EMAIL_BCON");
            foreach (string s in strings)
            {
                API.AddTextComponentSubstringPlayerName(s);
            }
            API.SetNotificationBackgroundColor((int)bgColor);
            API.DrawNotification(blink, saveToBrief);
        }
    }
}

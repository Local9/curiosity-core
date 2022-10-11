using CitizenFX.Core.UI;
using Curiosity.Framework.Shared.Enums;
using Curiosity.Framework.Shared.Models.Hud;
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

        internal static void ShowNotificationError(string message, bool blink = true, bool saveToBrief = true)
        {
            ShowNotification(message, blink, saveToBrief, eHudColor.HUD_COLOUR_RED);
        }

        internal static void ShowNotificationSuccess(string message, bool blink = true, bool saveToBrief = true)
        {
            ShowNotification(message, blink, saveToBrief, eHudColor.HUD_COLOUR_GREEN);
        }

        internal static void ShowNotification(string message, bool blink = true, bool saveToBrief = true, eHudColor bgColor = eHudColor.HUD_COLOUR_BLACK)
        {
            string[] strings = Screen.StringToArray(message);
            SetNotificationTextEntry("CELL_EMAIL_BCON");
            foreach (string s in strings)
            {
                AddTextComponentSubstringPlayerName(s);
            }
            SetNotificationBackgroundColor((int)bgColor);
            DrawNotification(blink, saveToBrief);
        }

        internal static MinimapAnchor GetMinimapAnchor()
        {
            var safezone = GetSafeZoneSize();
            var aspectRatio = GetAspectRatio(false);
            var resolutionX = 0;
            var resolutionY = 0;

            GetActiveScreenResolution(ref resolutionX, ref resolutionY);

            var scaleX = 1.0 / resolutionX;
            var scaleY = 1.0 / resolutionY;

            var anchor = new MinimapAnchor
            {
                Width = (float)(scaleX * (resolutionX / (4 * aspectRatio))),
                Height = (float)(scaleY * (resolutionY / 5.674)),
                X = (float)(scaleX * (resolutionX * (0.05f * (Math.Abs(safezone - 1.0) * 10)))),
                BottomY = 1.0 - scaleY * (resolutionY * (0.05f * (Math.Abs(safezone - 1.0) * 10)))
            };

            anchor.RightX = anchor.X + anchor.Width;
            anchor.Y = (float)(anchor.BottomY - anchor.Height);
            anchor.UnitX = (float)scaleX;
            anchor.UnitY = (float)scaleY;

            return anchor;
        }
    }
}

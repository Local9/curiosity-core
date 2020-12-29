using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Interface.Client.Interface;
using System.Threading.Tasks;

namespace Curiosity.Interface.Client
{
    public static class CommonFunctions
    {
        public static async Task Delay(int time)
        {
            await BaseScript.Delay(time);
        }

        public static void QuitSession() => API.NetworkSessionEnd(true, true);

        public static async void QuitGame()
        {
            Notify.Info("The game will exit in 5 seconds.");
            await BaseScript.Delay(5000);
            API.ForceSocialClubUpdate(); // bye bye
        }
        public static string[] StringToArray(string inputString)
        {
            return CitizenFX.Core.UI.Screen.StringToArray(inputString);
        }

        public static void DrawTextOnScreen(string text, float xPosition, float yPosition) =>
            DrawTextOnScreen(text, xPosition, yPosition, size: 0.48f);

        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size) =>
            DrawTextOnScreen(text, xPosition, yPosition, size, CitizenFX.Core.UI.Alignment.Left);

        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification) =>
            DrawTextOnScreen(text, xPosition, yPosition, size, justification, 6);

        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font) =>
            DrawTextOnScreen(text, xPosition, yPosition, size, justification, font, false);

        public static void DrawTextOnScreen(string text, float xPosition, float yPosition, float size, CitizenFX.Core.UI.Alignment justification, int font, bool disableTextOutline)
        {
            if (API.IsHudPreferenceSwitchedOn() && !API.IsPlayerSwitchInProgress() && API.IsScreenFadedIn() && !API.IsPauseMenuActive() && !API.IsFrontendFading() && !API.IsPauseMenuRestarting() && !API.IsHudHidden())
            {
                API.SetTextFont(font);
                API.SetTextScale(1.0f, size);
                if (justification == CitizenFX.Core.UI.Alignment.Right)
                {
                    API.SetTextWrap(0f, xPosition);
                }
                API.SetTextJustification((int)justification);
                if (!disableTextOutline) { API.SetTextOutline(); }
                API.BeginTextCommandDisplayText("STRING");
                API.AddTextComponentSubstringPlayerName(text);
                API.EndTextCommandDisplayText(xPosition, yPosition);
            }
        }
    }
}

using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Police.Client.Diagnostics;
using Curiosity.Police.Client.Interface;
using Curiosity.Systems.Library.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Police.Client
{
    public static class CommonFunctions
    {
        private static Vehicle _previousVehicle;

        public static async Task Delay(int time)
        {
            await BaseScript.Delay(time);
        }

        public static string GetVehDisplayNameFromModel(string name) => API.GetLabelText(API.GetDisplayNameFromVehicleModel((uint)API.GetHashKey(name)));
        public static bool DoesModelExist(string modelName) => DoesModelExist((uint)API.GetHashKey(modelName));
        public static bool DoesModelExist(uint modelHash) => API.IsModelInCdimage(modelHash);
        public static uint GetVehicleModel(int vehicle) => (uint)API.GetHashKey(API.GetEntityModel(vehicle).ToString());

        public static Vehicle GetVehicle(bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return Game.PlayerPed.LastVehicle;
            }
            else
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    return Game.PlayerPed.CurrentVehicle;
                }
            }
            return null;
        }
        public static Vehicle GetVehicle(Ped ped, bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return ped.LastVehicle;
            }
            else
            {
                if (ped.IsInVehicle())
                {
                    return ped.CurrentVehicle;
                }
            }
            return null;
        }

        public static Vehicle GetVehicle(Player player, bool lastVehicle = false)
        {
            if (lastVehicle)
            {
                return player.Character.LastVehicle;
            }
            else
            {
                if (player.Character.IsInVehicle())
                {
                    return player.Character.CurrentVehicle;
                }
            }
            return null;
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

        #region Load Model

        private static async Task<bool> LoadModel(uint modelHash)
        {
            // Check if the model exists in the game.
            if (API.IsModelInCdimage(modelHash))
            {
                // Load the model.
                API.RequestModel(modelHash);
                // Wait until it's loaded.
                while (!API.HasModelLoaded(modelHash))
                {
                    await Delay(0);
                }
                // Model is loaded, return true.
                return true;
            }
            // Model is not valid or is not loaded correctly.
            else
            {
                // Return false.
                return false;
            }
        }

        #endregion

        #region GetUserInput
        public static async Task<string> GetUserInput() => await GetUserInput(null, null, 30);
        public static async Task<string> GetUserInput(int maxInputLength) => await GetUserInput(null, null, maxInputLength);
        public static async Task<string> GetUserInput(string windowTitle) => await GetUserInput(windowTitle, null, 30);
        public static async Task<string> GetUserInput(string windowTitle, int maxInputLength) => await GetUserInput(windowTitle, null, maxInputLength);
        public static async Task<string> GetUserInput(string windowTitle, string defaultText) => await GetUserInput(windowTitle, defaultText, 30);
        public static async Task<string> GetUserInput(string windowTitle, string defaultText, int maxInputLength)
        {
            // Create the window title string.
            var spacer = "\t";
            API.AddTextEntry($"{API.GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", $"{windowTitle ?? "Enter"}:{spacer}(MAX {maxInputLength.ToString()} Characters)");

            // Display the input box.
            API.DisplayOnscreenKeyboard(1, $"{API.GetCurrentResourceName().ToUpper()}_WINDOW_TITLE", "", defaultText ?? "", "", "", "", maxInputLength);
            await Delay(0);
            // Wait for a result.
            while (true)
            {
                int keyboardStatus = API.UpdateOnscreenKeyboard();

                switch (keyboardStatus)
                {
                    case 3: // not displaying input field anymore somehow
                    case 2: // cancelled
                        return null;
                    case 1: // finished editing
                        return API.GetOnscreenKeyboardResult();
                    default:
                        await Delay(0);
                        break;
                }
            }
        }
        #endregion

        #region Animation Loading
        public static async Task LoadAnimationDict(string dict)
        {
            while (!API.HasAnimDictLoaded(dict))
            {
                API.RequestAnimDict(dict);
                await BaseScript.Delay(5);
            }
        }
        #endregion
    }
}

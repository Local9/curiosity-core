using CitizenFX.Core.Native;

namespace Curiosity.CasinoSystems.Client.Extensions
{
    class Helpers
    {
        public static void DisplayHelp(string text, int shape)
        {
            API.BeginTextCommandDisplayHelp(text);
            API.EndTextCommandDisplayHelp(0, false, true, shape);
        }

        public static void DisplayHelpWithNumber(string text, int number, int shape)
        {
            API.BeginTextCommandDisplayHelp(text);
            API.AddTextComponentInteger(number);
            API.EndTextCommandDisplayHelp(0, false, true, shape);
        }

        public static bool IsTextHelpBeingDisplayed(string message)
        {
            API.BeginTextCommandIsThisHelpMessageBeingDisplayed(message);
            return API.EndTextCommandIsThisHelpMessageBeingDisplayed(0);
        }
    }
}

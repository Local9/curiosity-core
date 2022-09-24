using Curiosity.Framework.Shared.Enums;

namespace Curiosity.Framework.Client.Utils
{
    internal class ScreenInterface
    {
        public static void StartLoadingMessage(string label, eBusySpinnerType eBusySpinnerType = eBusySpinnerType.BUSY_SPINNER_SAVE)
        {
            string textOutput = Game.GetGXTEntry(label);

            if (string.IsNullOrEmpty(textOutput))
                textOutput = label;

            SetLoadingPromptTextEntry("STRING");
            AddTextComponentSubstringPlayerName(textOutput);
            EndTextCommandBusyspinnerOn((int)eBusySpinnerType);
        }

        public static void StopLoadingMessage()
        {
            BusyspinnerOff();
        }

        public static void CloseLoadingScreen()
        {
            SetNuiFocus(false, false);
            ShutdownLoadingScreen();
            ShutdownLoadingScreenNui();
        }

        public static void EnableHud()
        {
            DisplayHud(true);
            DisplayRadar(true);
        }

        public static void DisableHud()
        {
            DisplayHud(false);
            DisplayRadar(false);
        }
    }
}

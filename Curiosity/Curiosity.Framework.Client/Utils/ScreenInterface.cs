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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using CitizenFX.Core.Native;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    class WorldTime
    {
        static Client client = Client.GetInstance();
        public static bool HideClock = false;

        public static void Init()
        {
            client.RegisterTickHandler(ShowTime);
        }

        static async Task ShowTime()
        {
            Vector2 position = new Vector2(0.135f, 0.945f);

            if (Screen.Resolution.Width > 1980)
            {
                position = new Vector2(0.232f, 0.945f);
            }

            int hours = API.GetClockHours();
            int minutes = API.GetClockMinutes();

            if (!HideClock)
                if (!CinematicMode.DoHideHud)
                    UI.DrawText($"{hours:00}:{minutes:00}", position, System.Drawing.Color.FromArgb(255, 255, 255, 255), 0.3f, Font.ChaletComprimeCologne);

            await Task.FromResult(0);
        }
    }
}

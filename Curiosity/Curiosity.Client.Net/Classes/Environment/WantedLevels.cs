using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Client.net.Classes.Environment
{
    static class WantedLevels
    {
        static bool IsWantedDisabled = false;
        static int MaxWantedLevel = 5;

        public static void Init()
        {
            Client.GetInstance().RegisterEventHandler("curiosity:Client:Settings:WantedDisabled", new Action<bool, int>(WantedDisabled));

            GetWantedSettings();
            WantedSettings();
        }

        static async void GetWantedSettings()
        {
            while (true)
            {
                BaseScript.TriggerServerEvent("curiosity:Server:Settings:Wanted");
                await BaseScript.Delay(60000);
            }
        }

        static void WantedDisabled(bool wanted, int maxWantedLevel)
        {
            IsWantedDisabled = wanted;
            MaxWantedLevel = maxWantedLevel;
        }

        static async void WantedSettings()
        {
            while (true)
            {
                if (IsWantedDisabled)
                {
                    API.ClearPlayerWantedLevel(Game.Player.Handle);
                    API.SetMaxWantedLevel(0);
                    API.SetPlayerWantedLevel(Game.Player.Handle, 0, false);
                    API.SetPlayerWantedLevelNow(Game.Player.Handle, false);
                    API.SetPlayerWantedLevelNoDrop(Game.Player.Handle, 0, false);
                    Game.Player.WantedLevel = 0;
                }
                else
                {
                    API.SetMaxWantedLevel(MaxWantedLevel);
                }
                await BaseScript.Delay(0);
            }
        }
    }
}

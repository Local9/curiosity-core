using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Curiosity.Shared.Client.net.Helper.ControlHelper;

using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;

namespace Curiosity.Client.Net
{
    public class CuriosityPed : BaseScript
    {
        bool isControlPressed = false;

        public CuriosityPed()
        {
            EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);

            Tick += Test;
        }

        async void OnResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;
            
            await Delay(0);
        }

        async Task Test()
        {
            if (IsControlPressed(Control.FrontendSocialClubSecondary))
            {
                Game.PlayerPed.Position = new Vector3(440.5f, -983.0f, 30.7f);
            }

            await Delay(0);
        }
    }
}

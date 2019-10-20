using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net.Helper;

namespace Curiosity.Client.net.Classes.Environment
{
    // TODO: Test and debug
    static class Voip
    {
        static SortedDictionary<float, string> voipRange = new SortedDictionary<float, string>()
        {
            [3.0f] = "Whispering",
            [17.0f] = "Low",
            [25.0f] = "Normal",
            [43.0f] = "Shouting"
        };
        static public KeyValuePair<float, string> currentRange = voipRange.ElementAt(2);

        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
            Function.Call(Hash.NETWORK_SET_TALKER_PROXIMITY, currentRange.Key);
        }

        static public Task OnTick()
        {
            if (ControlHelper.IsControlJustPressed(Control.EnterCheatCode, true))
            {
                currentRange = voipRange.ElementAt((voipRange.Keys.ToList().IndexOf(currentRange.Key) + 1) % (voipRange.Count));
                Screen.ShowNotification($"VOIP range set to ~b~{currentRange.Value}.");
                Function.Call(Hash.NETWORK_SET_TALKER_PROXIMITY, currentRange.Key);
            }

            //if (!UI.CinematicMode.DoHideHud)
            //{
            //    System.Drawing.Color colorInactive = System.Drawing.Color.FromArgb(200, 200, 200, 200);
            //    System.Drawing.Color colorActive = System.Drawing.Color.FromArgb(255, 138, 201, 38);

            //    UI.UI.DrawText($"Range: {currentRange.Value}", new Vector2(0.18f, 0.95f), ControlHelper.IsControlPressed(Control.PushToTalk, false, ControlModifier.Any) ? colorActive : colorInactive, 0.25f, 0, false);
            //}

            return Task.FromResult(0);
        }
    }
}

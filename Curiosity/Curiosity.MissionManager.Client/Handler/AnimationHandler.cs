using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Handler
{
    public class AnimationHandler
    {
        static public async void AnimationSearch()
        {
            string scenario = "PROP_HUMAN_BUM_BIN";
            if (!Game.PlayerPed.IsInVehicle())
            {
                API.TaskStartScenarioInPlace(Game.PlayerPed.Handle, scenario, 0, true);
                await PluginManager.Delay(5000);
                Game.PlayerPed.Task.ClearAll();
            }
        }
        static public async void AnimationClipboard()
        {
            string scenario = "WORLD_HUMAN_CLIPBOARD";
            if (!Game.PlayerPed.IsInVehicle())
            {
                API.TaskStartScenarioInPlace(Game.PlayerPed.Handle, scenario, 0, true);
                await PluginManager.Delay(5000);
                Game.PlayerPed.Task.ClearAll();
            }
        }

        static public async void AnimationRadio()
        {
            LoadAnimation("random@arrests");
            Game.PlayerPed.Task.PlayAnimation("random@arrests", "generic_radio_enter", 1.5f, 2.0f, -1, (AnimationFlags)50, 2.0f);
            await PluginManager.Delay(6000);
            Game.PlayerPed.Task.ClearAll();
        }

        static public async Task<bool> LoadAnimation(string dict)
        {
            while (!API.HasAnimDictLoaded(dict))
            {
                await PluginManager.Delay(0);
                API.RequestAnimDict(dict);
            }
            return true;
        }
    }
}

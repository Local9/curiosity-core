﻿using CitizenFX.Core;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.Helpers
{
    class Animations
    {
        static public async void AnimationSearch()
        {
            string scenario = "PROP_HUMAN_BUM_BIN";
            if (!Game.PlayerPed.IsInVehicle())
            {
                TaskStartScenarioInPlace(Game.PlayerPed.Handle, scenario, 0, true);
                await PluginManager.Delay(5000);
                Game.PlayerPed.Task.ClearAll();
            }
        }
        static public async void AnimationClipboard()
        {
            string scenario = "WORLD_HUMAN_CLIPBOARD";
            if (!Game.PlayerPed.IsInVehicle())
            {
                TaskStartScenarioInPlace(Game.PlayerPed.Handle, scenario, 0, true);
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
            while (!HasAnimDictLoaded(dict))
            {
                await PluginManager.Delay(0);
                RequestAnimDict(dict);
            }
            return true;
        }
    }
}

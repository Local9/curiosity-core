using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Shared.Client.net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.GameWorld.Client.net.Classes.Environment.UI
{
    static class CinematicMode
    {
        // To allow for the faster Invoke over DynamicInvoke, we require Tasks and Func instead of just using Delegate
        static List<Func<bool, Task>> callbacks = new List<Func<bool, Task>>();
        static public bool DoHideHud { get; set; } = false;
        static float blackBarHeight = 0.0f;
        static Client client = Client.GetInstance();

        static List<HudComponent> hideComponents = new List<HudComponent>()
        {
            HudComponent.WantedStars,
            HudComponent.WeaponIcon,
            HudComponent.Cash,
            HudComponent.MpCash,
            HudComponent.MpMessage,
            HudComponent.VehicleName,
            HudComponent.AreaName,
            HudComponent.Unused,
            HudComponent.StreetName,
            HudComponent.HelpText,
            HudComponent.FloatingHelpText1,
            HudComponent.FloatingHelpText2,
            HudComponent.CashChange,
            HudComponent.Reticle,
            HudComponent.SubtitleText,
            HudComponent.RadioStationsWheel,
            HudComponent.Saving,
            HudComponent.GamingStreamUnusde,
            //HudComponent.WeaponWheel, // I think this one caused players to be unable to switch weapons?
            HudComponent.WeaponWheelStats
        };

        static public Task RegisterCallback(Func<bool, Task> callback)
        {
            callbacks.Add(callback);
            return Task.FromResult(0);
        }

        static public void Init()
        {
            // May have to make this actually receive a string for an exported function instead
            // (Yet to be tested)
            //Exports.Add("CinematicMode.RegisterCallback", new Func<Func<bool, Task>, Task>(RegisterCallback));
            client.RegisterTickHandler(OnCinematicTick);
            client.RegisterEventHandler("curiosity:Player:UI:CinematicMode", new Action<bool>(HideHud));
            client.RegisterEventHandler("curiosity:Player:UI:BlackBarHeight", new Action(BlackBarHeight));
        }

        static public async Task OnCinematicTick()
        {
            if (DoHideHud)
            {
                hideComponents.ForEach(c => Screen.Hud.HideComponentThisFrame(c));
            }
            if (blackBarHeight > 0f)
            {
                Function.Call(Hash.DRAW_RECT, 0.5f, blackBarHeight / 2, 1f, blackBarHeight, 0, 0, 0, 255);
                Function.Call(Hash.DRAW_RECT, 0.5f, 1 - blackBarHeight / 2, 1f, blackBarHeight, 0, 0, 0, 255);
            }
            await Task.FromResult(0);
        }

        static public async void BlackBarHeight()
        {
            switch (blackBarHeight)
            {
                case 0f:
                    blackBarHeight = 0.15f;
                    break;
                case 0.15f:
                    blackBarHeight = 0.19f;
                    break;
                case 0.19f:
                    blackBarHeight = 0f;
                    break;
            }
            await BaseScript.Delay(0);
        }

        static public async void HideHud(bool state)
        {
            DoHideHud = state;
            BaseScript.TriggerEvent("curiosity:Client:Player:HideHud", DoHideHud);
            if (DoHideHud)
            {
                client.RegisterTickHandler(OnCinematicTick);
            }
            else
            {
                client.DeregisterTickHandler(OnCinematicTick);
            }
            callbacks.ForEach(cb => { cb.Invoke(!DoHideHud); });
            Function.Call(Hash.DISPLAY_RADAR, !DoHideHud);
            BaseScript.TriggerEvent("curiosity:Client:Chat:EnableChatBox", !DoHideHud);
            BaseScript.TriggerEvent("curiosity:Client:Player:DisplayInfo", !DoHideHud);
            await BaseScript.Delay(0);
        }
    }
}

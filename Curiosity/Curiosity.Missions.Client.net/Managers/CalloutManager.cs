using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.EventWrapper;
using Curiosity.Global.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Missions.Client.Managers
{
    public class CalloutManager : BaseScript
    {
        private static List<Type> registeredCallouts = new List<Type>();
        internal static Callout ActiveCallout { get; private set; }

        internal static long TimeCalloutCreated = 0;

        public CalloutManager()
        {
            EventHandlers[Events.Native.Client.OnClientResourceStart.Path] +=
                Events.Native.Client.OnClientResourceStart.Action +=
                    name =>
                    {
                        if (name != API.GetCurrentResourceName()) return;

                        // registeredCallouts.Add(typeof(StolenVehicle));
                    };
        }

        [Tick]
        private async Task OnCalloutUpdate()
        {
            if (ActiveCallout == null) return;

            if (TimeCalloutCreated == 0)
                TimeCalloutCreated = API.GetGameTimer();

            if ((API.GetGameTimer() - TimeCalloutCreated) > 5000 && !ActiveCallout.IsSetup)
            {
                Screen.ShowNotification($"Callout taking too long to create.");
                Screen.ShowNotification($"Did you miss the IsSetup Flag?");
            }

            if (!ActiveCallout.IsSetup) return;

            if (ActiveCallout.Players[0] == LocalPlayer) ActiveCallout.Tick();

            await Task.FromResult(100);
        }

        internal static void StartCallout()
        {
            TimeCalloutCreated = 0;
            var newCallout = Activator.CreateInstance(registeredCallouts.Random(), Game.Player) as Callout;
            newCallout?.Prepare();
            ActiveCallout = newCallout;
            if (ActiveCallout != null) ActiveCallout.Ended += forcefully => { ActiveCallout = null; };
        }
    }
}

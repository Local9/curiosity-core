using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Callouts.Client.Managers.Callouts;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.EventWrapper;
using Curiosity.Callouts.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Callouts.Client.Managers
{
    public class CalloutManager : BaseScript
    {
        private static List<Type> registeredCallouts = new List<Type>();
        internal static Callout ActiveCallout { get; private set; }

        internal static long TimeCalloutCreated = 0;

        public CalloutManager()
        {
            API.RegisterCommand("tco", new Action<int, List<object>, string>(OnRunCallout), false);

            EventHandlers[Events.Native.Client.OnClientResourceStart.Path] +=
                Events.Native.Client.OnClientResourceStart.Action +=
                    name =>
                    {
                        if (name != API.GetCurrentResourceName()) return;

                        registeredCallouts.Add(typeof(StolenVehicle));
                        registeredCallouts.Add(typeof(HostageSituation));
                        registeredCallouts.Add(typeof(ParkingViolation));
                    };
        }

        private void OnRunCallout(int playerId, List<object> args, string raw)
        {
#if DEBUG
            if (args.Count == 0)
            {
                return;
            }

            string calloutName = $"{args[0]}";

            Callout newCallout = null; // = Activator.CreateInstance(typeof(StolenVehicle), Game.Player) as Callout;

            switch(calloutName)
            {
                case "stolenVehicle":
                    newCallout = Activator.CreateInstance(typeof(StolenVehicle), Game.Player) as Callout;
                    break;
                case "hostage":
                    newCallout = Activator.CreateInstance(typeof(HostageSituation), Game.Player) as Callout;
                    break;
                case "parking":
                    newCallout = Activator.CreateInstance(typeof(ParkingViolation), Game.Player) as Callout;
                    break;
                default:
                    Screen.ShowNotification($"~r~Invalid callout type '{calloutName}'");
                    break;
            }

            if (ActiveCallout != null)
            {
                ActiveCallout?.End(true);
                ActiveCallout = null;
            }

            newCallout.Prepare();
            ActiveCallout = newCallout;

            if (ActiveCallout != null) ActiveCallout.Ended += forcefully => { ActiveCallout = null; };
#endif
        }

        [Tick]
        private async Task Update()
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

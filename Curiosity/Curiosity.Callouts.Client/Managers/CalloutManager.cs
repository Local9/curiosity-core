using CitizenFX.Core;
using CitizenFX.Core.Native;
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

        public CalloutManager()
        {
            EventHandlers[Events.Native.Client.OnClientResourceStart.Path] +=
                Events.Native.Client.OnClientResourceStart.Action +=
                    name =>
                    {
                        if (name != API.GetCurrentResourceName()) return;

                    };
        }

        [Tick]
        private async Task Update()
        {
            if (ActiveCallout == null) return;
            if (ActiveCallout.Players[0] == LocalPlayer) ActiveCallout.Tick();

            await Task.FromResult(0);
        }

        internal static void StartCallout()
        {
            var newCallout = Activator.CreateInstance(registeredCallouts.Random(), Game.Player) as Callout;
            newCallout?.Prepare();
            ActiveCallout = newCallout;
            if (ActiveCallout != null) ActiveCallout.Ended += forcefully => { ActiveCallout = null; };
        }
    }
}

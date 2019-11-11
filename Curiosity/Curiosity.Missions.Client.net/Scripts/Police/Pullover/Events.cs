using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace Curiosity.Missions.Client.net.Scripts.Police.Pullover
{
    class Events
    {
        public static event Events.OnPursuitEndedEvent OnPursuitEnded;
        public delegate void OnPursuitEndedEvent(Ped target);

        public static event Events.OnPursuitStartedEvent OnPursuitStarted;
        public delegate void OnPursuitStartedEvent(Ped target);

        internal static void InvokeOnPursuitStarted(Ped handle)
        {
            Events.OnPursuitStartedEvent eventHandler = Events.OnPursuitStarted;
            if (eventHandler == null)
            {
                return;
            }
            eventHandler(handle);
        }

        internal static void InvokeOnPursuitEnded(Ped handle)
        {
            Events.OnPursuitEndedEvent eventHandler = Events.OnPursuitEnded;
            if (eventHandler == null)
            {
                return;
            }
            eventHandler(handle);
        }
    }
}

using CitizenFX.Core.Native;
using Curiosity.Systems.Library.EventWrapperLegacy;
using Curiosity.Systems.Library.Models;
using System;

namespace Curiosity.Core.Client.Managers
{
    public class DutyManager : Manager<DutyManager>
    {
        public override void Begin()
        {
            Instance.EventRegistry[LegacyEvents.Client.CuriosityJob] += new Action<bool, bool, string>(OnDutyState);
        }

        private static void OnDutyState(bool active, bool onDuty, string job)
        {
            string msg = new JsonBuilder()
                .Add("operation", "JOB_ACTIVITY")
                .Add("jobActive", active)
                .Add("jobOnDuty", onDuty)
                .Add("jobTitle", job)
                .Build();

            API.SendNuiMessage(msg);
        }
    }
}

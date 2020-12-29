using CitizenFX.Core.Native;
using Curiosity.Systems.Library;
using Curiosity.Systems.Library.EventWrapperLegacy;
using Curiosity.Systems.Library.Models;
using System;

namespace Curiosity.Interface.Client.Managers
{
    public class DutyManager : Manager<DutyManager>
    {
        public override void Begin()
        {
            Instance.EventRegistry[LegacyEvents.Client.PoliceDutyEvent] += new Action<bool, bool, string>(OnDutyState);
        }

        private static void OnDutyState(bool active, bool onduty, string job)
        {
            string jsn = new JsonBuilder().Add("operation", "JOB_ACTIVITY")
                    .Add("jobActive", active)
                    .Add("jobOnDuty", onduty)
                    .Add("jobTitle", job.ToTitleCase())
                    .Build();

            API.SendNuiMessage(jsn);
        }
    }
}

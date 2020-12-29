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
            string jsn = new JsonBuilder().Add("operation", "DUTY")
                    .Add("isActive", active)
                    .Add("isDutyActive", onduty)
                    .Add("job", job.ToTitleCase())
                    .Build();

            API.SendNuiMessage(jsn);
        }
    }
}

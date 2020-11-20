using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.Data;
using Curiosity.Global.Shared.Enums;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Environment.Entities.Models;
using Curiosity.MissionManager.Client.Environment.Enums;
using Curiosity.Systems.Library.EventWrapperLegacy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Managers
{
    public class JobMarkerPoliceManager : Manager<JobMarkerPoliceManager>
    {
        List<Marker> markers = new List<Marker>();
        List<BlipData> blips = new List<BlipData>();

        System.Drawing.Color markerColor = System.Drawing.Color.FromArgb(255, 65, 105, 225);

        public override void Begin()
        {
            Logger.Info($"- [JobMarkerPoliceManager] Begin -----------------");

            markers.Add(new Marker("~b~Police Duty Registration", new Vector3(-1110.701f, -844.0428f, 18.31688f), markerColor, markerFilter: MarkerFilter.PoliceDuty));
            markers.Add(new Marker("~b~Police Duty Registration", new Vector3(441.0764f, -981.1343f, 29.6896f), markerColor, markerFilter: MarkerFilter.PoliceDuty));
            markers.Add(new Marker("~b~Police Duty Registration", new Vector3(622.13f, 17.52761f, 86.86286f), markerColor, markerFilter: MarkerFilter.PoliceDuty));
            markers.Add(new Marker("~b~Police Duty Registration", new Vector3(1851.431f, 3683.458f, 33.26704f), markerColor, markerFilter: MarkerFilter.PoliceDuty));
            markers.Add(new Marker("~b~Police Duty Registration", new Vector3(-448.4843f, 6008.57f, 30.71637f), markerColor, markerFilter: MarkerFilter.PoliceDuty));
            markers.Add(new Marker("~b~Police Duty Registration", new Vector3(368.8574f, -1610.202f, 28.29193f), markerColor, markerFilter: MarkerFilter.PoliceDuty));
            markers.Add(new Marker("~b~Police Duty Registration", new Vector3(826.2648f, -1288.504f, 27.24066f), markerColor, markerFilter: MarkerFilter.PoliceDuty));
            markers.Add(new Marker("~b~Police Duty Registration", new Vector3(-561.2444f, -132.6783f, 37.04274f), markerColor, markerFilter: MarkerFilter.PoliceDuty));

            int blipId = 0;

            foreach (Marker mark in markers)
            {
                BlipData blipData = new BlipData($"policeJobBlip{blipId}", "Police Duty", mark.Position, BlipSprite.PoliceStation, BlipCategory.Unknown, BlipColor.Blue, true);
                blips.Add(blipData);
                BlipHandler.AddBlip(blipData);

                MarkerManager.MarkersAll.Add(mark);
                blipId++;
            }
        }

        [TickHandler]
        private async Task OnPoliceJobMarkerTick()
        {
            Marker activeMarker = MarkerManager.GetActiveMarker(MarkerFilter.PoliceDuty);

            if (activeMarker == null)
            {
                await BaseScript.Delay(500);
                return;
            }

            string dutyText = JobManager.IsOfficer ? "~y~leave~w~" : "~y~join~w~";
            string job = JobManager.IsOfficer ? "unemployed" : "police";

            Screen.DisplayHelpTextThisFrame($"Press ~INPUT_PICKUP~ to {dutyText} the ~b~Police Force~w~.");

            if (Game.IsControlJustPressed(0, Control.Pickup))
            {
                
                BaseScript.TriggerEvent(LegacyEvents.Client.PoliceDutyEvent, true, false, job); // for legacy resources
                await PluginManager.Delay(5000);
            }
        }
    }
}

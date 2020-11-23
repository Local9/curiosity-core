using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.Data;
using Curiosity.Global.Shared.Enums;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Environment.Entities.Models;
using Curiosity.MissionManager.Client.Environment.Enums;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Manager;
using Curiosity.Systems.Library.EventWrapperLegacy;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Managers
{
    public class VehicleDeletionManager : Manager<VehicleDeletionManager>
    {
        EventSystem eventSystem => EventSystem.GetModule();

        List<Marker> markers = new List<Marker>();
        List<BlipData> blips = new List<BlipData>();

        System.Drawing.Color markerColor = System.Drawing.Color.FromArgb(255, 65, 105, 225);

        public override void Begin()
        {
            Logger.Info($"- [VehicleDeletionManager] Begin -----------------");

            markers.Add(new Marker("~b~Vehicle Deletion Point", new Vector3(-1110.701f, -844.0428f, 18.31688f), markerColor, markerFilter: MarkerFilter.VehicleDeletion));
            markers.Add(new Marker("~b~Vehicle Deletion Point", new Vector3(441.0764f, -981.1343f, 29.6896f), markerColor, markerFilter: MarkerFilter.VehicleDeletion));
            markers.Add(new Marker("~b~Vehicle Deletion Point", new Vector3(622.13f, 17.52761f, 86.86286f), markerColor, markerFilter: MarkerFilter.VehicleDeletion));
            markers.Add(new Marker("~b~Vehicle Deletion Point", new Vector3(1851.431f, 3683.458f, 33.26704f), markerColor, markerFilter: MarkerFilter.VehicleDeletion));
            markers.Add(new Marker("~b~Vehicle Deletion Point", new Vector3(-448.4843f, 6008.57f, 30.71637f), markerColor, markerFilter: MarkerFilter.VehicleDeletion));

            int blipId = 0;

            foreach (Marker mark in markers)
            {
                BlipData blipData = new BlipData($"vehicleDeletion{blipId}", "Vehicle Deletion", mark.Position, BlipSprite.Garage, BlipCategory.Unknown, BlipColor.Red, true);
                blips.Add(blipData);
                BlipHandler.AddBlip(blipData);

                MarkerManager.MarkersAll.Add(mark);
                blipId++;
            }
        }

        [TickHandler]
        private async Task OnVehicleDeletionMarker()
        {
            Marker activeMarker = MarkerManager.GetActiveMarker(MarkerFilter.VehicleDeletion);

            if (activeMarker == null)
            {
                await BaseScript.Delay(500);
                return;
            }

            Screen.DisplayHelpTextThisFrame($"~w~Press ~INPUT_PICKUP~ to ~o~Delete ~w~your ~b~Vehicle~w~.");

            if (Game.IsControlJustPressed(0, Control.Pickup))
            {
                if (PlayerManager.PersonalVehicle == null) return;

                if (PlayerManager.PersonalVehicle.Exists())
                {
                    eventSystem.Request<bool>("vehicle:delete", PlayerManager.PersonalVehicle.NetworkId);
                }

                await PluginManager.Delay(5000);
            }
        }
    }
}

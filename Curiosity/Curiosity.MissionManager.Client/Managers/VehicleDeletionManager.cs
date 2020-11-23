using CitizenFX.Core;
using CitizenFX.Core.Native;
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

        System.Drawing.Color markerColor = System.Drawing.Color.FromArgb(255, 226, 110, 110);
        Vector3 scale = new Vector3(4f, 4f, 1f);

        public override void Begin()
        {
            Logger.Info($"- [VehicleDeletionManager] Begin -----------------");

            markers.Add(new Marker("~r~Vehicle Deletion Point", new Vector3(462.7421f, -1019.469f, 27.1043f), markerColor, scale, markerFilter: MarkerFilter.VehicleDeletion));
            markers.Add(new Marker("~r~Vehicle Deletion Point", new Vector3(463.3558f, -1015.196f, 27.07401f), markerColor, scale, markerFilter: MarkerFilter.VehicleDeletion));
            markers.Add(new Marker("~r~Vehicle Deletion Point", new Vector3(-356.0414f, -115.358f, 37.69658f), markerColor, scale, markerFilter: MarkerFilter.VehicleDeletion));
            markers.Add(new Marker("~r~Vehicle Deletion Point", new Vector3(2366.087f, 3167.133f, 46.84825f), markerColor, scale, markerFilter: MarkerFilter.VehicleDeletion));
            markers.Add(new Marker("~r~Vehicle Deletion Point", new Vector3(-449.2261f, 6052.743f, 30.34053f), markerColor, scale, markerFilter: MarkerFilter.VehicleDeletion));

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

            if (!Game.PlayerPed.IsInVehicle())
            {
                await BaseScript.Delay(500);
                return;
            }

            if (activeMarker == null)
            {
                await BaseScript.Delay(500);
                return;
            }

            Screen.DisplayHelpTextThisFrame($"~w~Press ~INPUT_PICKUP~ to ~o~Delete ~w~your ~b~Vehicle~w~.");

            if (Game.IsControlJustPressed(0, Control.Pickup))
            {
                Vehicle vehicle = Game.PlayerPed.CurrentVehicle;

                Game.PlayerPed.Task.WarpOutOfVehicle(vehicle);

                API.NetworkFadeOutEntity(vehicle.Handle, false, false);

                await PluginManager.Delay(500);

                eventSystem.Request<bool>("vehicle:delete", vehicle.NetworkId);

                await PluginManager.Delay(500);

                if (vehicle.Exists())
                    vehicle.Delete();

                await PluginManager.Delay(5000);
            }
        }
    }
}

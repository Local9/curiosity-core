using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.Data;
using Curiosity.Global.Shared.Enums;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Environment.Entities.Models;
using Curiosity.MissionManager.Client.Environment.Enums;
using Curiosity.MissionManager.Client.Events;
using Curiosity.MissionManager.Client.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Managers
{
    public class VehicleRepairManager : Manager<VehicleRepairManager>
    {
        EventSystem eventSystem => EventSystem.GetModule();

        List<Marker> markers = new List<Marker>();
        List<BlipData> blips = new List<BlipData>();

        System.Drawing.Color markerColor = System.Drawing.Color.FromArgb(255, 135, 206, 235);
        Vector3 scale = new Vector3(4f, 4f, 1f);

        public override void Begin()
        {
            Logger.Info($"- [VehicleRepairManager] Begin -------------------");

            markers.Add(new Marker("~r~Vehicle Repair Point", new Vector3(1870.172f, 3693.716f, 33.6004f), markerColor, scale, markerFilter: MarkerFilter.VehicleRepair));
            markers.Add(new Marker("~r~Vehicle Repair Point", new Vector3(138.3844f, 6634.493f, 31.64594f), markerColor, scale, markerFilter: MarkerFilter.VehicleRepair));
            markers.Add(new Marker("~r~Vehicle Repair Point", new Vector3(1183.209f, 2648.456f, 36.84267f), markerColor, scale, markerFilter: MarkerFilter.VehicleRepair));
            markers.Add(new Marker("~r~Vehicle Repair Point", new Vector3(-356.0485f, -114.0199f, 37.69672f), markerColor, scale, markerFilter: MarkerFilter.VehicleRepair));
            markers.Add(new Marker("~r~Vehicle Repair Point", new Vector3(717.1935f, -1078.147f, 21.25965f), markerColor, scale, markerFilter: MarkerFilter.VehicleRepair));
            markers.Add(new Marker("~r~Vehicle Repair Point", new Vector3(-199.5225f, -1380.813f, 30.25824f), markerColor, scale, markerFilter: MarkerFilter.VehicleRepair));

            int blipId = 0;

            foreach (Marker mark in markers)
            {
                BlipData blipData = new BlipData($"vehicleRepair{blipId}", "Vehicle Repair", mark.Position, BlipSprite.Repair, BlipCategory.Unknown, BlipColor.Green, true);
                blips.Add(blipData);
                BlipHandler.AddBlip(blipData);

                MarkerManager.MarkersAll.Add(mark);
                blipId++;
            }
        }

        [TickHandler]
        private async Task OnVehicleDeletionMarker()
        {
            Marker activeMarker = MarkerManager.GetActiveMarker(MarkerFilter.VehicleRepair);

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

            if (Game.PlayerPed.CurrentVehicle.Driver != Game.PlayerPed)
            {
                await BaseScript.Delay(500);
                return;
            }

            Screen.DisplayHelpTextThisFrame($"~w~Press ~INPUT_PICKUP~ to ~o~Repair ~w~your ~b~Vehicle~w~.");

            if (Game.IsControlJustPressed(0, Control.Pickup))
            {
                Vehicle vehicle = Game.PlayerPed.CurrentVehicle;

                bool response = await eventSystem.Request<bool>("vehicle:repair");

                if (response)
                {
                    vehicle.Repair();
                    vehicle.Wash();
                    vehicle.ClearLastWeaponDamage();
                    Notify.Success($"Vehicle repaired");
                }
                else
                {
                    Notify.Alert($"Vehicle not repaired");
                }

                await PluginManager.Delay(5000);
            }
        }
    }
}

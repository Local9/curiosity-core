
using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Missions.Client.net.MissionVehicles;
using Curiosity.Missions.Client.net.MissionVehicleTypes;

namespace Curiosity.Missions.Client.net.Scripts.VehicleCreators
{
    static class CreateVehicles
    {
        public static InteractiveVehicle TrafficStop(Vehicle vehicle)
        {
            Screen.ShowNotification($"~b~Traffic Stop: ~w~Initiated");

            InteractiveVehicle interactiveVehicle;

            vehicle.Driver.SetConfigFlag(281, true);
            vehicle.Driver.AlwaysKeepTask = true;
            vehicle.Driver.BlockPermanentEvents = true;
            vehicle.IsPersistent = true;

            interactiveVehicle = new TrafficStopVehicle(vehicle.Handle);

            return interactiveVehicle;
        }
    }
}

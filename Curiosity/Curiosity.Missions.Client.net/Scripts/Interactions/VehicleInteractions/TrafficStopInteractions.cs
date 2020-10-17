using CitizenFX.Core;
using System;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.Scripts.Interactions.VehicleInteractions
{
    static class TrafficStopInteractions
    {
        static public async void TrafficStopVehicleFlee(this Vehicle vehicle, Ped ped)
        {
            try
            {
                TaskSetBlockingOfNonTemporaryEvents(ped.Handle, false);
                ped.SetConfigFlag(292, false);
                vehicle.IsEngineRunning = true;
                SetVehicleCanBeUsedByFleeingPeds(vehicle.Handle, true);

                if (vehicle.Driver == null)
                {
                    ped.Task.EnterVehicle(vehicle, VehicleSeat.Driver, 20000, 5f);
                }

                await PluginManager.Delay(1000);

                int willRam = PluginManager.Random.Next(5);

                if (willRam == 4)
                {
                    TaskVehicleTempAction(vehicle.Driver.Handle, vehicle.Handle, 28, 3000);
                }

                await PluginManager.Delay(0);

                TaskVehicleTempAction(vehicle.Driver.Handle, vehicle.Handle, 32, 30000);
                vehicle.Driver.Task.FleeFrom(Game.PlayerPed);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TrafficStopVehicleFlee -> {ex}");
            }
        }
    }
}

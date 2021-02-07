using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Utils;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class VehicleManager : Manager<VehicleManager>
    {
        private const string DECOR_VEH_FUEL = "Vehicle.Fuel";
        private const string STATE_VEH_FUEL = "VEHICLE_FUEL";
        private const string STATE_VEH_FUEL_MULTIPLIER = "VEHICLE_FUEL_MULTIPLIER";
        private const string STATE_VEH_FUEL_SETUP = "VEHICLE_FUEL_SETUP";
        Vehicle currentVehicle;

        static Dictionary<VehicleClass, float> FuelConsumptionClassMultiplier = new Dictionary<VehicleClass, float>()
        {
            [VehicleClass.Planes] = 3.5f,
            [VehicleClass.Helicopters] = 1.4f,
            [VehicleClass.Super] = 2.5f,
            [VehicleClass.Sports] = 1.8f,
            [VehicleClass.Emergency] = 2.0f,
            [VehicleClass.Industrial] = 1.8f,
            [VehicleClass.Commercial] = 1.8f
        };

        static float minRandomFuel = 14f;
        static float maxRandomFuel = 97f;

        public override void Begin()
        {
            // spawn
            // delete

            API.DecorRegister(DECOR_VEH_FUEL, 1);

            EventSystem.Attach("delete:vehicle", new EventCallback(metadata =>
            {
                Logger.Debug("delete vehicle");

                Vehicle vehicle = Cache.PersonalVehicle;

                if (Game.PlayerPed.IsInVehicle())
                    vehicle = Game.PlayerPed.CurrentVehicle;

                if (vehicle != null)
                    EventSystem.Send("delete:entity", vehicle.NetworkId);
                    
                return null;
            }));

            EventSystem.Attach("repair:vehicle", new EventCallback(metadata =>
            {
                Logger.Debug("repair vehicle");

                Vehicle vehicle = Cache.PersonalVehicle;

                if (Game.PlayerPed.IsInVehicle())
                    vehicle = Game.PlayerPed.CurrentVehicle;

                if (vehicle != null)
                {
                    vehicle.Wash();
                    vehicle.Repair();
                    vehicle.EngineHealth = 1000f;
                    vehicle.BodyHealth = 1000f;
                    vehicle.PetrolTankHealth = 1000f;
                    vehicle.Health = vehicle.MaxHealth;
                    vehicle.ClearLastWeaponDamage();

                    Notify.Success($"Vehicle Repaired");
                }
                else
                {
                    Notify.Alert($"Must be in a vehicle");
                }

                return null;
            }));

            // edit
        }

        internal async void InitialiseVehicleFuel(Vehicle veh)
        {
            if (veh.Driver != Game.PlayerPed) return;

            currentVehicle = veh;

            bool setup = currentVehicle.State.Get(STATE_VEH_FUEL_SETUP);

            while (!setup)
            {
                await BaseScript.Delay(0);
                currentVehicle.State.Set(STATE_VEH_FUEL_SETUP, true, true);
            }

            float randomFuel = (float)(minRandomFuel + (maxRandomFuel - minRandomFuel) * (Utility.RANDOM.NextDouble()));
            currentVehicle.State.Set(STATE_VEH_FUEL, randomFuel, true);

            float classMultiplier = FuelConsumptionClassMultiplier[veh.ClassType];
            currentVehicle.State.Set(STATE_VEH_FUEL_MULTIPLIER, classMultiplier, true);

            PluginManager.Instance.AttachTickHandler(OnVehicleFuel);
        }

        private async Task OnVehicleFuel()
        {
            if (!Game.PlayerPed.IsInVehicle())
            {
                PluginManager.Instance.DetachTickHandler(OnVehicleFuel);
            }

            if (ControlHelper.IsControlPressed(Control.VehicleAccelerate, false) && !Game.PlayerPed.CurrentVehicle.IsEngineRunning)
                Game.PlayerPed.CurrentVehicle.IsEngineRunning = true;

            int fuel = (int)currentVehicle.State.Get(STATE_VEH_FUEL);
            float multi = (float)currentVehicle.State.Get(STATE_VEH_FUEL_MULTIPLIER);

            ScreenInterface.Draw3DText(currentVehicle.Position, $"F: {fuel}, FM: {multi}");

            API.DecorSetFloat(currentVehicle.Handle, DECOR_VEH_FUEL, fuel); // LEGACY
        }
    }
}

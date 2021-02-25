using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Utils;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class VehicleManager : Manager<VehicleManager>
    {
        private const string DECOR_VEH_FUEL = "Vehicle.Fuel";
        private const string STATE_VEH_FUEL = "VEHICLE_FUEL";
        private const string STATE_VEH_FUEL_MULTIPLIER = "VEHICLE_FUEL_MULTIPLIER";
        private const string STATE_VEH_FUEL_SETUP = "VEHICLE_FUEL_SETUP";
        public const string STATE_VEH_SPAWNED = "VEH_SPAWNED";
        private const float FUEL_PUMP_RANGE = 6f;

        private uint GAS_STATION_TESLA = 2140883938;

        Vehicle currentVehicle;

        Dictionary<VehicleClass, float> FuelConsumptionClassMultiplier = new Dictionary<VehicleClass, float>()
        {
            [VehicleClass.Planes] = 3.5f,
            [VehicleClass.Helicopters] = 1.4f,
            [VehicleClass.Super] = 2.5f,
            [VehicleClass.Sports] = 1.8f,
            [VehicleClass.Emergency] = 2.0f,
            [VehicleClass.Industrial] = 1.8f,
            [VehicleClass.Commercial] = 1.8f,
            [(VehicleClass)22] = 4.0f // OpenWheel
        };

        Dictionary<VehicleHash, float> FuelConsumptionModelMultiplier = new Dictionary<VehicleHash, float>()
        {
            // [VehicleHash.Infernus] = 100f // For testing
        };

        List<VehicleHash> EletricVehicles = new List<VehicleHash>();

        float minRandomFuel = 14f;
        float maxRandomFuel = 97f;
        private int currentUpdate = -1;
        private int lastUpdate = -1;
        private bool IsRefueling = false;
        private bool IsNearFuelPump = false;
        private bool IsAwaitingServerResponse = false;

        List<ObjectHash> FuelPumpModelHashes = new List<ObjectHash>()
        {
            ObjectHash.prop_gas_pump_1a,
            ObjectHash.prop_gas_pump_1b,
            ObjectHash.prop_gas_pump_1c,
            ObjectHash.prop_gas_pump_1d,
            ObjectHash.prop_gas_pump_old2,
            ObjectHash.prop_gas_pump_old3,
            ObjectHash.prop_vintage_pump
        };

        public override void Begin()
        {
            // spawn
            // delete

            int veh1 = AddVehicle("p90d", .5f);
            if (veh1 > 0)
                EletricVehicles.Add((VehicleHash)veh1);

            int veh2 = AddVehicle("teslasemi", .5f);
            if (veh2 > 0)
                EletricVehicles.Add((VehicleHash)veh2);

            int veh3 = AddVehicle("tezeract", .5f);
            if (veh3 > 0)
                EletricVehicles.Add((VehicleHash)veh3);

            int stationHash = API.GetHashKey("teslasupercharger");
            if (stationHash > 0)
                GAS_STATION_TESLA = (uint)stationHash;

            API.DecorRegister(DECOR_VEH_FUEL, 1);

            EventSystem.Attach("delete:vehicle", new EventCallback(metadata =>
            {
                Logger.Debug("delete vehicle");

                Vehicle vehicle = Cache.PersonalVehicle;

                if (Cache.PlayerPed.IsInVehicle())
                    vehicle = Cache.PlayerPed.CurrentVehicle;

                if (vehicle != null)
                    EventSystem.Send("delete:entity", vehicle.NetworkId);
                    
                return null;
            }));

            EventSystem.Attach("repair:vehicle", new EventCallback(metadata =>
            {
                Logger.Debug("repair vehicle");

                Vehicle vehicle = Cache.PersonalVehicle;

                if (Cache.PlayerPed.IsInVehicle())
                    vehicle = Cache.PlayerPed.CurrentVehicle;

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

        private int AddVehicle(string vehicle, float fuelMultiplier)
        {
            int handle = API.GetHashKey(vehicle);

            if (handle > 0)
            {
                FuelConsumptionModelMultiplier.Add((VehicleHash)handle, fuelMultiplier);
                return handle;
            }
            return 0;
        }

        internal async void InitialiseVehicleFuel(Vehicle veh)
        {
            if (veh.Driver != Cache.PlayerPed) return;

            currentVehicle = veh;

            bool setup = false;

            if (currentVehicle.State.Get(STATE_VEH_FUEL_SETUP) != null)
                setup = currentVehicle.State.Get(STATE_VEH_FUEL_SETUP);

            if (!setup)
            {
                Logger.Debug($"VFM: {currentVehicle.Handle}:{setup}");

                if (currentVehicle.State.Get(STATE_VEH_SPAWNED))
                {
                    minRandomFuel = 100f;
                    maxRandomFuel = 100f;
                }

                float randomFuel = (float)(minRandomFuel + (maxRandomFuel - minRandomFuel) * (Utility.RANDOM.NextDouble()));
                currentVehicle.State.Set(STATE_VEH_FUEL, randomFuel, true);

                float classMultiplier = 1 / 1600f;
                classMultiplier *= (FuelConsumptionClassMultiplier.ContainsKey(veh.ClassType) ? FuelConsumptionClassMultiplier[veh.ClassType] : 1.0f);
                classMultiplier *= 1.0f;
                currentVehicle.State.Set(STATE_VEH_FUEL_MULTIPLIER, classMultiplier, true);

                currentVehicle.State.Set(STATE_VEH_FUEL_SETUP, true, true);
            }

            PluginManager.Instance.AttachTickHandler(OnVehicleFuel);
            PluginManager.Instance.AttachTickHandler(OnVehicleRefuel);
            PluginManager.Instance.AttachTickHandler(CheckFuelPumpDistance);
        }

        private bool IsNearNormalFuelPump()
        {
            return World.GetAllProps().Where(o => FuelPumpModelHashes.Contains((ObjectHash)o.Model.Hash)).Any(o => o.Position.DistanceToSquared(Cache.PlayerPed.Position) < Math.Pow(2 * FUEL_PUMP_RANGE, 2));
        }

        private async Task CheckFuelPumpDistance()
        {
            try
            {
                await BaseScript.Delay(500);
                if (Cache.PlayerPed.IsInVehicle())
                {
                    if (currentVehicle != null)
                    {
                        if (EletricVehicles.Contains((VehicleHash)currentVehicle.Model.Hash))
                        {
                            IsNearFuelPump = World.GetAllProps().Where(o => (ObjectHash)GAS_STATION_TESLA == (ObjectHash)o.Model.Hash).Any(o => o.Position.DistanceToSquared(Cache.PlayerPed.Position) < Math.Pow(2 * FUEL_PUMP_RANGE, 2));
                        }
                        else
                        {
                            IsNearFuelPump = IsNearNormalFuelPump();
                        }
                    }
                    else
                    {
                        IsNearFuelPump = IsNearNormalFuelPump();
                    }
                }
                else
                {
                    IsNearFuelPump = false;
                    IsRefueling = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CheckFuelPumpDistance() -> {ex.Message}");
            }
        }

        private async Task OnVehicleRefuel()
        {
            if (IsNearFuelPump && !IsRefueling && !IsAwaitingServerResponse)
            {
                Screen.DisplayHelpTextThisFrame("Press ~INPUT_REPLAY_START_STOP_RECORDING_SECONDARY~ to ~y~Refuel ~s~the ~b~Vehicle");
                ScreenInterface.Draw3DText(currentVehicle.Position, "~w~Press ~b~F2 ~w~to ~y~Refuel ~s~the ~b~Vehicle~n~~w~~b~X ~w~Button on Controller", 40);

                if (ControlHelper.IsControlJustPressed(Control.ReplayStartStopRecordingSecondary, false) && Cache.PlayerPed.IsInVehicle())
                {
                    IsAwaitingServerResponse = true;
                    IsRefueling = true;

                    currentVehicle.IsEngineRunning = false;

                    float fuel = (float)currentVehicle.State.Get(STATE_VEH_FUEL);

                    bool success = await EventSystem.Request<bool>("vehicle:refuel:charge", fuel);

                    if (success)
                    {
                        currentVehicle.State.Set(STATE_VEH_FUEL, 100f, true);
                        Notify.Success($"Vehicle Refueled");
                    }

                    IsAwaitingServerResponse = false;
                    IsRefueling = false;
                    currentVehicle.IsEngineRunning = true;

                    await BaseScript.Delay(5000);
                }
            }
            else
            {
                await BaseScript.Delay(2000);
            }
        }

        private async Task OnVehicleFuel()
        {
            if (!Cache.PlayerPed.IsInVehicle())
            {
                lastUpdate = -1;
                PluginManager.Instance.DetachTickHandler(OnVehicleFuel);
                PluginManager.Instance.DetachTickHandler(OnVehicleRefuel);
                PluginManager.Instance.DetachTickHandler(CheckFuelPumpDistance);
                IsNearFuelPump = false;
                IsRefueling = false;
                currentVehicle = null;
                return;
            }

            float fuel = (float)currentVehicle.State.Get(STATE_VEH_FUEL);
            float multi = (float)currentVehicle.State.Get(STATE_VEH_FUEL_MULTIPLIER);

            if (fuel == 0f && !IsRefueling)
            {
                currentVehicle.IsEngineRunning = false;
                currentVehicle.FuelLevel = 0f;

                API.DecorSetFloat(currentVehicle.Handle, DECOR_VEH_FUEL, fuel); // LEGACY
            }

            if (ControlHelper.IsControlPressed(Control.VehicleAccelerate, false) && !currentVehicle.IsEngineRunning)
            {
                currentVehicle.IsEngineRunning = true;
                IsRefueling = false;
            }

            if (IsRefueling)
            {
                await BaseScript.Delay(500);
                return;
            }

            if (lastUpdate == -1)
                lastUpdate = API.GetGameTimer();

            currentUpdate = API.GetGameTimer();
            double deltaTime = (currentUpdate - lastUpdate) / 1000f;
            float vehicleSpeed = Math.Abs(currentVehicle.Speed);

            if (vehicleSpeed < 4f) // drain fuel while parked
                vehicleSpeed = 30f;

            if (!currentVehicle.IsEngineRunning)
                vehicleSpeed = 0f;

            if (currentVehicle.IsEngineRunning)
            {
                fuel = Math.Max(0f, fuel - (float)(deltaTime * multi * vehicleSpeed));
                currentVehicle.FuelLevel = fuel;
                currentVehicle.State.Set(STATE_VEH_FUEL, fuel, true);
                API.DecorSetFloat(currentVehicle.Handle, DECOR_VEH_FUEL, fuel); // LEGACY
            }

            lastUpdate = currentUpdate;

            // Screen.ShowSubtitle($"F: {fuel:0.00000}, FM: {multi:0.00000}, VC: {currentVehicle.ClassType}");

            await BaseScript.Delay(500);
        }
    }
}

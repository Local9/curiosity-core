using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Client.net.Classes.Menus;
using Curiosity.Client.net.Helpers;
using Curiosity.Global.Shared.net.Enums;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Curiosity.Client.net.Models;
using Curiosity.Shared.Client.net.Helper;

namespace Curiosity.Client.net.Classes.Vehicle
{
    // To set fuel on a car when taken out of a garage, set the Vehicle.Fuel decor on it.
    // TODO: Potentially make the vehicles have different fuel tank sizes
    static class FuelManager
    {
        static float startingMultiplier = 1 / 1600f;
        static float FuelPumpRange = 4f;

        // For random vehicles with unassigned fuel levels
        static float minRandomFuel = 14f;
        static float maxRandomFuel = 97f;

        // Just placeholders for testing, feel free to change
        static Dictionary<VehicleClass, float> FuelConsumptionClassMultiplier = new Dictionary<VehicleClass, float>()
        {
            [VehicleClass.Planes] = 1.4f,
            [VehicleClass.Helicopters] = 1.4f,
            [VehicleClass.Super] = 1.2f,
            [VehicleClass.Sports] = 1.2f
        };

        static Dictionary<VehicleHash, float> FuelConsumptionModelMultiplier = new Dictionary<VehicleHash, float>()
        {
            //[VehicleHash.Infernus] = 100f // For testing
        };

        static List<ObjectHash> FuelPumpModelHashes = new List<ObjectHash>()
        {
            ObjectHash.prop_gas_pump_1a,
            ObjectHash.prop_gas_pump_1b,
            ObjectHash.prop_gas_pump_1c,
            ObjectHash.prop_gas_pump_1d,
            ObjectHash.prop_gas_pump_old2,
            ObjectHash.prop_gas_pump_old3,
            ObjectHash.prop_vintage_pump
        };

        static List<Vector3> GasStations = new List<Vector3>()
        {
            new Vector3(49.4187f, 2778.793f, 58.043f),
            new Vector3(263.894f, 2606.463f, 44.983f),
            new Vector3(1039.958f, 2671.134f, 39.550f),
            new Vector3(1207.260f, 2660.175f, 37.899f),
            new Vector3(2539.685f, 2594.192f, 37.944f),
            new Vector3(2679.858f, 3263.946f, 55.240f),
            new Vector3(2005.055f, 3773.887f, 32.403f),
            new Vector3(1687.156f, 4929.392f, 42.078f),
            new Vector3(1701.314f, 6416.028f, 32.763f),
            new Vector3(179.857f, 6602.839f, 31.868f),
            new Vector3(-94.4619f, 6419.594f, 31.489f),
            new Vector3(-2554.996f, 2334.40f, 33.078f),
            new Vector3(-1800.375f, 803.661f, 138.651f),
            new Vector3(-1437.622f, -276.747f, 46.207f),
            new Vector3(-2096.243f, -320.286f, 13.168f),
            new Vector3(-724.619f, -935.1631f, 19.213f),
            new Vector3(-526.019f, -1211.003f, 18.184f),
            new Vector3(-70.2148f, -1761.792f, 29.534f),
            new Vector3(265.648f, -1261.309f, 29.292f),
            new Vector3(819.653f, -1028.846f, 26.403f),
            new Vector3(1208.951f, -1402.567f, 35.224f),
            new Vector3(1181.381f, -330.847f, 69.316f),
            new Vector3(620.843f, 269.100f, 103.089f),
            new Vector3(2581.321f, 362.039f, 108.468f),
            new Vector3(176.631f, -1562.025f, 29.263f),
            new Vector3(176.631f, -1562.025f, 29.263f),
            new Vector3(-319.292f, -1471.715f, 30.549f),
            new Vector3(1784.324f, 3330.55f, 41.253f)
        };

        static float fuelUsageMultiplier = -1;
        public static float vehicleFuel = -1;
        static private int currentUpdate = -1;
        static private int lastUpdate;
        static ObjectList ObjectList = new ObjectList();

        static Client client = Client.GetInstance();

        private static Random random = new Random();
        private static double PlayerToVehicleRefuelRange = 5f;
        private static bool isNearFuelPump;
        private static int cooldown = 0;

        private static bool IsInstantRefuelDisabled = false;

        static public void Init()
        {
            Function.Call(Hash.DECOR_REGISTER, "Vehicle.Fuel", 1);
            Function.Call(Hash.DECOR_REGISTER, "Vehicle.FuelUsageMultiplier", 1);

            client.RegisterTickHandler(PeriodicCheck);
            client.RegisterTickHandler(GasStationBlips);
            UpdateSettings();
            CheckFuelPumpDistance();

            client.RegisterEventHandler("curiosity:Client:Vehicle:Refuel", new Action(ClientRefuel));
            client.RegisterEventHandler("curiosity:Client:Vehicle:GetCurrentFuelLevel", new Action(GetCurrentFuelLevel));
            client.RegisterEventHandler("curiosity:Client:Settings:InstantRefuel", new Action<bool>(InstantRefuel));
        }

        static async Task GasStationBlips()
        {
            Blip currentGasBlip = null;
            while (true)
            {
                await BaseScript.Delay(10000);
                isNearFuelPump = false;

                if (Game.PlayerPed.IsInHeli || Game.PlayerPed.IsInBoat || Game.PlayerPed.IsInPlane || !Game.PlayerPed.IsInVehicle() || Game.PlayerPed.CurrentVehicle.ClassType == VehicleClass.Cycles)
                {
                    if (currentGasBlip != null)
                    {
                        if (currentGasBlip.Exists())
                        {
                            currentGasBlip.Delete();
                        }
                    }
                }
                else
                {

                    Vector3 playerPos = Game.PlayerPed.Position;
                    float closest = 1000.0f;
                    Vector3 closestCoords = new Vector3();

                    foreach (Vector3 gasStation in GasStations)
                    {
                        float distanceCheck = NativeWrappers.GetDistanceBetween(playerPos, gasStation, false);

                        if (distanceCheck < closest)
                        {
                            if (distanceCheck < 10f)
                                isNearFuelPump = true;

                            closest = distanceCheck;
                            closestCoords = gasStation;
                        }
                    }

                    if (currentGasBlip != null)
                    {
                        if (currentGasBlip.Exists())
                        {
                            currentGasBlip.Delete();
                        }
                    }

                    currentGasBlip = new Blip(API.AddBlipForCoord(closestCoords.X, closestCoords.Y, closestCoords.Z));
                    currentGasBlip.Sprite = BlipSprite.JerryCan;
                    currentGasBlip.Color = BlipColor.Red;
                    currentGasBlip.Scale = 0.9f;
                    currentGasBlip.IsShortRange = true;
                }
            }
        }

        static async void UpdateSettings()
        {
            while (true)
            {
                BaseScript.TriggerServerEvent("curiosity:Server:Settings:InstantRefuel");
                await BaseScript.Delay(60000);
            }
        }

        static async void CheckFuelPumpDistance()
        {
            while (true)
            {
                try
                {
                    await BaseScript.Delay(250);
                    isNearFuelPump = ObjectList.Select(o => new Prop(o)).Where(o => FuelPumpModelHashes.Contains((ObjectHash)(uint)o.Model.Hash)).Any(o => o.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(2 * FuelPumpRange, 2));
                    if (isNearFuelPump)
                        Debug.WriteLine($"{DateTime.Now}: Near Pump: {isNearFuelPump}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"CheckFuelPumpDistance() -> {ex.Message}");
                }
            }
        }

        /// <summary>
        /// </summary>
        static async Task PeriodicCheck()
        {
            try
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    if (!Function.Call<bool>(Hash.DECOR_EXIST_ON, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.Fuel"))
                    {
                        // For very large random float numbers this method does not yield a uniform distribution
                        // But for this magnitude it is perfectly fine
                        float randomFuel = (float)(minRandomFuel + (maxRandomFuel - minRandomFuel) * (random.NextDouble()));
                        Function.Call(Hash._DECOR_SET_FLOAT, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.Fuel", randomFuel);
                    }
                    vehicleFuel = Function.Call<float>(Hash._DECOR_GET_FLOAT, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.Fuel");

                    if (!Function.Call<bool>(Hash.DECOR_EXIST_ON, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.FuelUsageMultiplier"))
                    {
                        fuelUsageMultiplier = startingMultiplier;
                        //Log.ToChat($"{fuelUsageMultiplier:0.00000}");
                        VehicleClass VehicleClass = (VehicleClass)Function.Call<int>(Hash.GET_VEHICLE_CLASS, Game.PlayerPed.CurrentVehicle.Handle);
                        fuelUsageMultiplier *= (FuelConsumptionClassMultiplier.ContainsKey(VehicleClass) ? FuelConsumptionClassMultiplier[VehicleClass] : 1.0f);
                        fuelUsageMultiplier *= FuelConsumptionModelMultiplier.ContainsKey((VehicleHash)(uint)Game.PlayerPed.CurrentVehicle.Model.Hash) ? FuelConsumptionModelMultiplier[(VehicleHash)(uint)Game.PlayerPed.CurrentVehicle.Model.Hash] : 1f;
                        Function.Call(Hash._DECOR_SET_FLOAT, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.FuelUsageMultiplier", fuelUsageMultiplier);
                    }
                    if (lastUpdate == -1)
                    {
                        lastUpdate = Function.Call<int>(Hash.GET_GAME_TIMER);
                    }
                    if (fuelUsageMultiplier < 0)
                    {
                        fuelUsageMultiplier = Function.Call<float>(Hash._DECOR_GET_FLOAT, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.FuelUsageMultiplier");
                    }
                    currentUpdate = Function.Call<int>(Hash.GET_GAME_TIMER);
                    double deltaTime = (currentUpdate - lastUpdate) / 1000f;
                    float vehicleSpeed = Math.Abs(Game.PlayerPed.CurrentVehicle.Speed);

                    if (vehicleSpeed < 4f)
                    {
                        vehicleSpeed = 4f;
                    }

                    if (!Game.PlayerPed.CurrentVehicle.IsEngineRunning)
                    {
                        vehicleSpeed = 0f;
                    }

                    vehicleFuel = Math.Max(0f, vehicleFuel - (float)(deltaTime * fuelUsageMultiplier * vehicleSpeed));

                    Function.Call(Hash._DECOR_SET_FLOAT, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.Fuel", vehicleFuel);
                    lastUpdate = currentUpdate;
                }
                else
                {
                    fuelUsageMultiplier = -1;
                    //vehicleFuel = -1;
                    lastUpdate = -1;
                }
                //try
                //{
                //    isNearFuelPump = ObjectList.Select(o => new Prop(o)).Where(o => FuelPumpModelHashes.Contains((ObjectHash)(uint)o.Model.Hash)).Any(o => o.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(2 * FuelPumpRange, 2));
                //}
                //catch (Exception ex)
                //{
                //    Debug.WriteLine($"FuelManager isNearFuelPump Error: {ex.Message}");
                //    isNearFuelPump = false;
                //}
                await BaseScript.Delay(500);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FuelManager Error: {ex.Message}");
                await BaseScript.Delay(500);
            }
        }

        public static async void Refuel(float amount)
        {
            try
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    Environment.UI.Notifications.LifeV(1, "Vehicle", "Refuel", "You can't refuel while in your vehicle!", 8);
                    return;
                }

                amount = Math.Max(0f, amount); // Selling gas to gas stations... Why not?
                amount = Math.Min(100f - vehicleFuel, amount);
                var NearbyVehicles = new VehicleList().Select(v => (CitizenFX.Core.Vehicle)Entity.FromHandle(v)).Where(v => v.Bones["wheel_rr"].Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(PlayerToVehicleRefuelRange, 2)).OrderBy(v => v.Bones["wheel_rr"].Position.DistanceToSquared(Game.PlayerPed.Position));
                if (!NearbyVehicles.Any())
                {
                    Environment.UI.Notifications.LifeV(1, "Vehicle", "Refuel", "You are not close enough to a vehicle.", 8);
                    return;
                }
                CitizenFX.Core.Vehicle vehicle = NearbyVehicles.First();

                var NearbyPumps = ObjectList.Select(o => new Prop(o)).Where(o => FuelPumpModelHashes.Contains((ObjectHash)(uint)o.Model.Hash)).Where(o => o.Position.DistanceToSquared(vehicle.Position) < Math.Pow(FuelPumpRange, 2));
                if (!NearbyPumps.Any())
                {
                    Environment.UI.Notifications.LifeV(1, "Vehicle", "Refuel", "You are not close enough to a pump.", 8);
                    return;
                }

                int refuelTick = 1;
                float refuelTickAmount = 0.07f;
                float refuelRate = 0.35f;
                float refueled = 0f;

                vehicleFuel = Function.Call<float>(Hash._DECOR_GET_FLOAT, vehicle.Handle, "Vehicle.Fuel");
                if (vehicleFuel >= 0f) // -1f used as null
                {
                    Vector3 startingPosition = vehicle.Position;
                    while (refueled < amount)
                    {
                        if (startingPosition != vehicle.Position)
                        {
                            Environment.UI.Notifications.LifeV(1, "Vehicle", "Refuel", "Your vehicle moved while refuelling.", 8);
                            return;
                        }

                        //if too far away, print and return
                        refueled += refuelTickAmount;
                        vehicleFuel += refuelTickAmount;
                        await BaseScript.Delay(refuelTick);
                        vehicleFuel = Math.Min(100f, vehicleFuel);
                        Function.Call(Hash._DECOR_SET_FLOAT, vehicle.Handle, "Vehicle.Fuel", vehicleFuel);
                    }

                    Environment.UI.Notifications.LifeV(1, "Vehicle", "Refuel", "You have finished refuelling.", 20);
                }
            }
            catch (Exception ex)
            {
                //Log.Error($"FuelManager Refuel Error: {ex.Message}");
            }
        }

        static async void DevRefuel()
        {
            Function.Call(Hash._DECOR_SET_FLOAT, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.Fuel", 100f);

            await BaseScript.Delay(0);
        }

        static async void ClientRefuel()
        {
            if (IsInstantRefuelDisabled)
            {
                return;
            }

            Function.Call(Hash._DECOR_SET_FLOAT, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.Fuel", 100f);

            await BaseScript.Delay(0);
        }

        static async void GetCurrentFuelLevel()
        {
            BaseScript.TriggerEvent("curiosity:Client:Vehicle:CurrentFuel", vehicleFuel);
            await BaseScript.Delay(0);
        }

        static void InstantRefuel(bool IsInstantRefuelDisabledSetting)
        {
            IsInstantRefuelDisabled = IsInstantRefuelDisabledSetting;
        }

        ///// <summary>
        ///// Refuels the local player's vehicle
        ///// </summary>
        ///// <param name="amount"></param>
        ///// <returns>Whether the refuel was successful or not</returns>
        //static public void HandleRefuel(Command command)
        //{
        //    try
        //    {
        //        float amount;
        //        if (command.Args.Count == 1)
        //        {
        //            amount = 100f;
        //            paymentType = command.Args.Get(0).ToLower() == "debit" ? MoneyType.Debit : MoneyType.Cash;
        //        }
        //        else if(command.Args.Count == 2)
        //        {
        //            amount = command.Args.GetFloat(0);
        //            paymentType = command.Args.Get(1).ToLower() == "debit" ? MoneyType.Debit : MoneyType.Cash;
        //        }
        //        else
        //        {
        //            BaseScript.TriggerEvent("curiosity:Client:Chat:Message", "", "#CC7777", "You need to specify either cash or debit, e.g. /refuel cash or refuel 30 debit.");
        //            return;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error($"FuelManager HandleRefuel Error: {ex.Message}");
        //    }
        //}
    }
}

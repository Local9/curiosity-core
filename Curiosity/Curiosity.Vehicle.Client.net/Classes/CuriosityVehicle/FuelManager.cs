using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.Entity;
using Curiosity.Global.Shared.Enums;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Vehicles.Client.net.Classes.CurPlayer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Debug = CitizenFX.Core.Debug;

namespace Curiosity.Vehicles.Client.net.Classes.CuriosityVehicle
{
    // To set fuel on a car when taken out of a garage, set the Vehicle.Fuel decor on it.
    // TODO: Potentially make the vehicles have different fuel tank sizes
    static class FuelManager
    {
        static float startingMultiplier = 1 / 1600f;
        static float FuelPumpRange = 6f;
        static bool refueling = false;

        // For random vehicles with unassigned fuel levels
        static float minRandomFuel = 14f;
        static float maxRandomFuel = 97f;

        static bool warning1Played = false;
        static bool warning2Played = false;

        static Blip currentGasBlip = null;

        // Just placeholders for testing, feel free to change
        static Dictionary<VehicleClass, float> FuelConsumptionClassMultiplier = new Dictionary<VehicleClass, float>()
        {
            [VehicleClass.Planes] = 5f,
            [VehicleClass.Helicopters] = 1.4f,
            [VehicleClass.Super] = 2.5f,
            [VehicleClass.Sports] = 1.8f,
            [VehicleClass.Emergency] = 2.0f,
            [VehicleClass.Industrial] = 1.8f,
            [VehicleClass.Commercial] = 1.8f
        };

        static Dictionary<VehicleHash, float> FuelConsumptionModelMultiplier = new Dictionary<VehicleHash, float>()
        {
            // [VehicleHash.Infernus] = 100f // For testing
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
            new Vector3(-319.292f, -1471.715f, 30.549f),
            new Vector3(1784.324f, 3330.55f, 41.253f)
        };

        static float fuelUsageMultiplier = -1;
        public static float vehicleFuel = -1;
        static private int currentUpdate = -1;
        static private int lastUpdate;

        static Plugin client = Plugin.GetInstance();

        private static Random random = new Random();
        private static double PlayerToVehicleRefuelRange = 5f;
        private static bool isNearFuelPump;
        private static int cooldown = 0;

        private static long GameTimer;

        private static bool IsInstantRefuelDisabled = false;
        private static bool IsFuelFree = false;

        static public void Init()
        {
            Function.Call(Hash.DECOR_REGISTER, "Vehicle.Fuel", 1);
            Function.Call(Hash.DECOR_REGISTER, "Vehicle.FuelUsageMultiplier", 1);

            client.RegisterTickHandler(PeriodicCheckRefuel);
            client.RegisterTickHandler(GasStationBlips);
            client.RegisterTickHandler(ShowHelpText);

            CheckFuelPumpDistance();

            GameTimer = API.GetGameTimer();

            client.RegisterEventHandler("playerSpawn", new Action<dynamic>(OnPlayerSpawn));

            client.RegisterEventHandler("curiosity:Client:Vehicle:Refuel", new Action(ClientRefuel));
            client.RegisterEventHandler("curiosity:Client:Vehicle:GetCurrentFuelLevel", new Action(GetCurrentFuelLevel));
            client.RegisterEventHandler("curiosity:Client:Settings:InstantRefuel", new Action<bool>(InstantRefuel));
            client.RegisterEventHandler("curiosity:Client:Settings:Chargeable", new Action<bool>(Chargeable));

            //DevRefuel
            client.RegisterEventHandler("curiosity:Client:Vehicle:DevRefuel", new Action(DevRefuel));
        }

        private static void AddVehicle(string vehicle, float fuelMultiplier)
        {
            int handle = API.GetHashKey(vehicle);

            if (handle > 0)
                FuelConsumptionModelMultiplier.Add((VehicleHash)handle, fuelMultiplier);
        }

        static void OnPlayerSpawn(dynamic dynData)
        {
            OnUpdateSettings();

            AddVehicle("p90d", .5f);
            AddVehicle("teslasemi", .5f);
        }

        static async void OnUpdateSettings()
        {
            while (true)
            {
                BaseScript.TriggerServerEvent("curiosity:Server:Settings:InstantRefuel");
                await Plugin.Delay(1000 * 60);
            }
        }

        static async Task GasStationBlips()
        {
            if (IsFuelFree)
            {
                if (currentGasBlip != null)
                {
                    if (currentGasBlip.Exists())
                    {
                        currentGasBlip.Delete();
                    }
                }
                client.DeregisterTickHandler(GasStationBlips);
            }

            await BaseScript.Delay(10000);

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
                currentGasBlip.Name = "Gas Station";
            }
        }

        static async Task ShowHelpText()
        {
            if (isNearFuelPump && !refueling)
            {
                NativeWrappers.DrawHelpText("Press ~INPUT_REPLAY_START_STOP_RECORDING_SECONDARY~ to ~y~refuel vehicle");


                if (isNearFuelPump && ControlHelper.IsControlJustPressed(Control.ReplayStartStopRecordingSecondary, false) && Game.PlayerPed.IsInVehicle() && Game.PlayerPed.CurrentVehicle.Driver.IsPlayer)
                {
                    Refuel(100.0f - vehicleFuel);
                    await BaseScript.Delay(5000);
                }
            }
            else
            {
                await BaseScript.Delay(2000);
            }
        }

        static async void CheckFuelPumpDistance()
        {
            while (true)
            {
                try
                {
                    await BaseScript.Delay(500);
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        isNearFuelPump = World.GetAllProps().Where(o => FuelPumpModelHashes.Contains((ObjectHash)o.Model.Hash)).Any(o => o.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(2 * FuelPumpRange, 2));
                    }
                    else
                    {
                        isNearFuelPump = false;
                        refueling = false;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"CheckFuelPumpDistance() -> {ex.Message}");
                }
            }
        }

        /// <summary>
        /// </summary>
        static async Task PeriodicCheckRefuel()
        {
            try
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    if (ControlHelper.IsControlPressed(Control.VehicleAccelerate, false) && !Game.PlayerPed.CurrentVehicle.IsEngineRunning && isNearFuelPump)
                        Game.PlayerPed.CurrentVehicle.IsEngineRunning = true;

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
                        // CitizenFX.Core.UI.Screen.ShowSubtitle($"{fuelUsageMultiplier:0.00000}");
                        VehicleClass VehicleClass = (VehicleClass)Function.Call<int>(Hash.GET_VEHICLE_CLASS, Game.PlayerPed.CurrentVehicle.Handle);
                        fuelUsageMultiplier *= (FuelConsumptionClassMultiplier.ContainsKey(VehicleClass) ? FuelConsumptionClassMultiplier[VehicleClass] : 1.0f);
                        fuelUsageMultiplier *= FuelConsumptionModelMultiplier.ContainsKey((VehicleHash)(uint)Game.PlayerPed.CurrentVehicle.Model.Hash) ? FuelConsumptionModelMultiplier[(VehicleHash)(uint)Game.PlayerPed.CurrentVehicle.Model.Hash] : 1.0f;
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
                        vehicleSpeed = 30f;
                    }

                    if (!Game.PlayerPed.CurrentVehicle.IsEngineRunning)
                    {
                        vehicleSpeed = 0f;
                    }

                    if (Game.PlayerPed.CurrentVehicle.IsEngineRunning)
                    {
                        vehicleFuel = Math.Max(0f, vehicleFuel - (float)(deltaTime * fuelUsageMultiplier * vehicleSpeed));
                        Function.Call(Hash._DECOR_SET_FLOAT, Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.Fuel", vehicleFuel);
                    }

                    if ((API.GetGameTimer() - GameTimer) < 1000)
                    {
                        GameTimer = API.GetGameTimer();

                        if (vehicleFuel > 20f && vehicleFuel < 35f && !warning1Played)
                        {
                            PlayWarning(false);
                            warning1Played = true;
                            Screen.ShowNotification($"~o~Fuel Warning");
                        }
                            

                        if (vehicleFuel > 1f && vehicleFuel < 15f && !warning2Played)
                        {
                            PlayWarning(true);
                            warning2Played = true;
                            Screen.ShowNotification($"~r~Critical Fuel Warning");
                        }
                            
                    }

                    lastUpdate = currentUpdate;
                }
                else
                {
                    fuelUsageMultiplier = -1;
                    //vehicleFuel = -1;
                    lastUpdate = -1;
                }
                await BaseScript.Delay(500);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FuelManager Error: {ex.Message}");
                await BaseScript.Delay(500);
            }
        }

        static async void PlayWarning(bool final)
        {
            string warning = final ? "5_SEC_WARNING" : "10_SEC_WARNING";

            API.PlaySoundFrontend(-1, warning, "HUD_MINI_GAME_SOUNDSET", true);
            await BaseScript.Delay(500);
            API.PlaySoundFrontend(-1, warning, "HUD_MINI_GAME_SOUNDSET", true);
            await BaseScript.Delay(500);
            API.PlaySoundFrontend(-1, warning, "HUD_MINI_GAME_SOUNDSET", true);
            await BaseScript.Delay(500);
            API.PlaySoundFrontend(-1, warning, "HUD_MINI_GAME_SOUNDSET", true);
            await BaseScript.Delay(500);
            API.PlaySoundFrontend(-1, warning, "HUD_MINI_GAME_SOUNDSET", true);
        }

        public static async void Refuel(float amount)
        {
            try
            {
                CitizenFX.Core.Vehicle vehicle = Game.PlayerPed.CurrentVehicle;

                if (vehicle.Driver != Game.PlayerPed)
                {
                    Plugin.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Vehicle", "Refuel", "Must be the driver to refuel vehicle.", 8);
                    return;
                }

                if (refueling)
                {
                    Plugin.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Vehicle", "Refuel", "Currently Refueling.", 8);
                    return;
                }

                refueling = true;

                PlayerInformationModel playerInfo = PlayerInformation.playerInfo;

                int cashTotal = playerInfo.Wallet + playerInfo.BankAccount;

                if (cashTotal < (int)amount)
                {
                    Plugin.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Vehicle", "Refuel", "You don't have enough money.", 8);
                    return;
                }

                //if (Game.PlayerPed.IsInVehicle())
                //{
                //    Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Vehicle", "Refuel", "You can't refuel while in your vehicle!", 8);
                //    return;
                //}

                Game.PlayerPed.CurrentVehicle.IsEngineRunning = false;

                amount = Math.Max(0f, amount); // Selling gas to gas stations... Why not?
                amount = Math.Min(100f - vehicleFuel, amount);

                var NearbyPumps = World.GetAllProps().Where(o => FuelPumpModelHashes.Contains((ObjectHash)o.Model.Hash)).Where(o => o.Position.DistanceToSquared(vehicle.Position) < Math.Pow(FuelPumpRange, 2));
                if (!NearbyPumps.Any())
                {
                    refueling = false;
                    Plugin.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Vehicle", "Refuel", "You are not close enough to a pump.", 8);
                    return;
                }

                int refuelTick = 25;
                float refuelTickAmount = 0.07f;
                float refueled = 0f;

                vehicleFuel = Function.Call<float>(Hash._DECOR_GET_FLOAT, vehicle.Handle, "Vehicle.Fuel");
                if (vehicleFuel >= 0f) // -1f used as null
                {
                    Vector3 startingPosition = vehicle.Position;
                    while (refueled < amount)
                    {
                        // Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Vehicle", "Refuel", "Tank is filling", 20);
                        CitizenFX.Core.UI.Screen.ShowSubtitle("~s~Vehicle is ~g~refuelling.");
                        vehicleFuel = Function.Call<float>(Hash._DECOR_GET_FLOAT, vehicle.Handle, "Vehicle.Fuel");
                        if (startingPosition != vehicle.Position)
                        {
                            refueling = false;
                            Plugin.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Vehicle", "Refuel", "Your vehicle moved while refuelling. You can try refuelling again.", 8);
                            CitizenFX.Core.UI.Screen.ShowSubtitle("~s~Vehicle is ~r~no longer refueling.");
                            Charge((int)(refueled));
                            return;
                        }

                        if (Game.PlayerPed.CurrentVehicle.IsEngineRunning)
                        {
                            refueling = false;
                            Plugin.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Vehicle", "Refuel", "Your vehicles engine started while refuelling.", 8);
                            CitizenFX.Core.UI.Screen.ShowSubtitle("~s~Vehicle is ~r~no longer refuelling.");
                            Charge((int)(refueled));
                            return;
                        }

                        //if too far away, print and return
                        refueled += refuelTickAmount;
                        vehicleFuel += refuelTickAmount;
                        await BaseScript.Delay(refuelTick);
                        vehicleFuel = Math.Min(100.00f, vehicleFuel);
                        Function.Call(Hash._DECOR_SET_FLOAT, vehicle.Handle, "Vehicle.Fuel", vehicleFuel);
                    }
                    await BaseScript.Delay(0);
                    Plugin.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Vehicle", "Refuel", "You have finished refuelling.", 20);
                    CitizenFX.Core.UI.Screen.ShowSubtitle("~s~Vehicle is ~r~no longer refuelling.");
                    Game.PlayerPed.CurrentVehicle.IsEngineRunning = true;
                    refueling = false;

                    Charge((int)(refueled));

                    if (vehicleFuel > 15f)
                    {
                        warning2Played = false;
                    }

                    if (vehicleFuel > 50f)
                    {
                        warning1Played = false;
                    }
                }
            }
            catch (Exception ex)
            {
                //Log.Error($"FuelManager Refuel Error: {ex.Message}");
            }
        }

        static async void Charge(int cost)
        {
            if (IsFuelFree)
            {
                // Free refueling
                return;
            }

            cost = (int)(cost * 1.5);

            PlayerInformationModel playerInfo = PlayerInformation.playerInfo;

            if ((playerInfo.Wallet - cost) > 0)
            {
                Plugin.TriggerServerEvent("curiosity:Server:Bank:DecreaseCash", playerInfo.Wallet, cost);
            }
            else if ((playerInfo.BankAccount - cost) > 0)
            {
                Plugin.TriggerServerEvent("curiosity:Server:Bank:DecreaseBank", playerInfo.BankAccount, cost);
            }
            else
            {
                // Put bank into debt
                Plugin.TriggerServerEvent("curiosity:Server:Bank:DecreaseBank", playerInfo.BankAccount, cost);
            }

            await BaseScript.Delay(0);
        }

        static void DevRefuel()
        {
            if (!PlayerInformation.IsDeveloper()) return;
            API.DecorSetFloat(Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.Fuel", 100f);
            Game.PlayerPed.CurrentVehicle.IsEngineRunning = true;
            warning1Played = false;
            warning2Played = false;
        }

        static async void ClientRefuel()
        {
            if (IsInstantRefuelDisabled)
            {
                return;
            }

            float currentFuel = API.DecorGetFloat(Plugin.CurrentVehicle.Handle, "Vehicle.Fuel");

            Charge((int)(100f - currentFuel));
            warning1Played = false;
            warning2Played = false;

            Function.Call(Hash._DECOR_SET_FLOAT, Plugin.CurrentVehicle.Handle, "Vehicle.Fuel", 100f);

            await BaseScript.Delay(0);
        }

        static async void GetCurrentFuelLevel()
        {
            BaseScript.TriggerEvent("curiosity:Client:Vehicle:CurrentFuel", vehicleFuel);
            await BaseScript.Delay(0);
        }

        static void Chargeable(bool setting)
        {
            IsFuelFree = setting;

            if (PlayerInformation.IsDeveloper())
            {
                Debug.WriteLine($"IsInstantRefuelDisabled: {IsInstantRefuelDisabled}");
            }
        }

        static void InstantRefuel(bool IsInstantRefuelDisabledSetting)
        {
            IsInstantRefuelDisabled = IsInstantRefuelDisabledSetting;

            if (PlayerInformation.IsDeveloper())
            {
                Debug.WriteLine($"IsInstantRefuelDisabled: {IsInstantRefuelDisabled}");
            }
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

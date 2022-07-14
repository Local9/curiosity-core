﻿using CitizenFX.Core.UI;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Settings;
using Curiosity.Core.Client.State;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System.Linq;
using LibUtils = Curiosity.Systems.Library.Utils;

namespace Curiosity.Core.Client.Managers
{
    public class VehicleManager : Manager<VehicleManager>
    {
        private const string DECOR_VEH_FUEL = "Vehicle.Fuel";
        private const string DECOR_VEH_FUEL_USE = "Vehicle.Fuel.Use";
        private const float FUEL_PUMP_RANGE = 6f;
        private const float FUEL_PUMP_RANGE_BOAT = 30f;
        private uint GAS_STATION_TESLA = 2140883938;
        bool _canSpawn = true;
        NotificationManager NotificationManager => NotificationManager.GetModule();

        public long GameTimeLeftVehicle;

        JobManager jobManager => JobManager.GetModule();

        VehicleState currentVehicle;

        Dictionary<VehicleClass, float> FuelConsumptionClassMultiplier = new Dictionary<VehicleClass, float>()
        {
            [VehicleClass.Planes] = 2.8f,
            [VehicleClass.Helicopters] = 1.4f,
            [VehicleClass.Super] = 3.6f,
            [VehicleClass.Sports] = 3.2f,
            [VehicleClass.Emergency] = 2.4f,
            [VehicleClass.Industrial] = 2.4f,
            [VehicleClass.Commercial] = 2.4f,
            [VehicleClass.Boats] = 2.4f,
            [(VehicleClass)22] = 4.5f // OpenWheel
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

        const float ELECTRIC_VEHICLE_FUEL_MODIFIER = .5f;

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

        public static bool EnableInverseTorque = false;

        public override void Begin()
        {
            GameTimeLeftVehicle = GetGameTimer();
            // spawn
            // delete

            foreach (string vehicle in ConfigurationManager.GetModule().EletricVehicles())
            {
                AddVehicle(vehicle, ELECTRIC_VEHICLE_FUEL_MODIFIER);
            }

            int stationHash = API.GetHashKey("teslasupercharger");
            if (stationHash > 0)
                GAS_STATION_TESLA = (uint)stationHash;

            API.DecorRegister(DECOR_VEH_FUEL, 1);
            API.DecorRegister(DECOR_VEH_FUEL_USE, 1);

            EventSystem.Attach("delete:vehicle", new EventCallback(metadata =>
            {
                Logger.Debug("delete vehicle");

                Vehicle vehicle = Cache.PersonalVehicle is not null ? Cache.PersonalVehicle.Vehicle : Game.PlayerPed.CurrentVehicle;

                if (Cache.PlayerPed.IsInVehicle())
                    vehicle = Cache.PlayerPed.CurrentVehicle;

                if (vehicle != null)
                {
                    if (vehicle.AttachedBlip is not null)
                        vehicle.AttachedBlip.Delete();

                    int vehicleBlip = API.GetBlipFromEntity(vehicle.Handle);
                    API.RemoveBlip(ref vehicleBlip);

                    EntityManager.GetModule().RemoveEntityBlip(vehicle);

                    EventSystem.Send("delete:entity", vehicle.NetworkId);
                }

                return null;
            }));

            EventSystem.Attach("repair:vehicle", new AsyncEventCallback(async metadata =>
            {
                Logger.Debug("repair vehicle");

                bool canRepair = await EventSystem.Request<bool>("vehicle:repair");

                if (!canRepair)
                {
                    Interface.Notify.Info($"To not have enough cash to repair the vehicle.");
                    return null;
                }

                Vehicle vehicle = Cache.PersonalVehicle.Vehicle;

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

                    NotificationManager.GetModule().Success("Vehicle Repaired", "bottom-middle");
                }
                else
                {
                    NotificationManager.GetModule().Warn("Must be in a Vehicle", "bottom-middle");
                }

                return null;
            }));

            // edit
        }

        const int JERRY_CAN_MAX = 4500;

        [TickHandler(SessionWait = true)]
        private async Task OnRefuelWithJerryCan()
        {
            CitizenFX.Core.Weapon currentWeapon = Cache.PlayerPed.Weapons.Current;

            if (currentWeapon.Hash != WeaponHash.PetrolCan)
            {
                await BaseScript.Delay(500);
                return;
            }

            int currentAmmo = currentWeapon.Ammo;
            Ped playerPed = Cache.PlayerPed;

            if (currentAmmo > 0 && currentVehicle is not null)
            {
                Vector3 boneCoord1 = currentVehicle.Vehicle.Bones["wheel_lr"].Position;
                Vector3 boneCoord2 = currentVehicle.Vehicle.Bones["petroltank_l"].Position;

                if (playerPed.IsInRangeOf(boneCoord1, 1.7f) || playerPed.IsInRangeOf(boneCoord2, 1.7f))
                {
                    Screen.DisplayHelpTextThisFrame("Hold ~INPUT_CONTEXT~ to refuel your vehicle.");
                    if (Game.IsControlPressed(0, Control.Context) && currentVehicle.Vehicle.FuelLevel < 100f)
                    {
                        float adjustedFuelLevel = currentVehicle.Vehicle.FuelLevel += 0.1f;
                        currentVehicle.Vehicle.FuelLevel = adjustedFuelLevel;
                        currentVehicle.Vehicle.State.Set(StateBagKey.VEH_FUEL, adjustedFuelLevel, true);
                        API.DecorSetFloat(currentVehicle.Vehicle.Handle, DECOR_VEH_FUEL, adjustedFuelLevel); // LEGACY

                        currentWeapon.Ammo -= 6;

                        Vector3 pos = Vector3.Subtract(currentVehicle.Vehicle.Position, playerPed.Position);
                        float heading = pos.ToHeading();
                        playerPed.Heading = heading;
                        playerPed.Task.PlayAnimation("weapon@w_sp_jerrycan", "fire", 1f, 600, AnimationFlags.UpperBodyOnly);
                    }
                }
            }

            if (currentAmmo < JERRY_CAN_MAX && IsNearNormalFuelPump())
            {
                Screen.DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to refuel your Jerry Can.");
                if (Game.IsControlJustPressed(0, Control.Context))
                {
                    GenericMessage result = await EventSystem.Request<GenericMessage>("vehicle:refuel:jerry", JERRY_CAN_MAX - currentAmmo);
                    if (result.Success)
                    {
                        currentWeapon.Ammo = JERRY_CAN_MAX;
                        NotificationManager.Success($"Jerry Can refueled (${result.Cost})", "bottom-middle");
                    }

                    if (!result.Success)
                        NotificationManager.Info($"{result.Message}", "bottom-middle");

                    await BaseScript.Delay(1000);
                }
            }
        }

        private int AddVehicle(string vehicle, float fuelMultiplier)
        {
            int handle = API.GetHashKey(vehicle);

            if (handle > 0)
            {
                VehicleHash vehicleHash = (VehicleHash)handle;

                FuelConsumptionModelMultiplier.Add(vehicleHash, fuelMultiplier);
                if (!EletricVehicles.Contains(vehicleHash))
                    EletricVehicles.Add(vehicleHash);

                return handle;
            }
            return 0;
        }

        #region Vehicle Fuel
        internal async void InitialiseVehicleFuel(VehicleState veh)
        {
            await BaseScript.Delay(500);

            if (veh.Vehicle.Driver != Cache.PlayerPed) return;

            currentVehicle = veh;

            bool isSetup = false;
            int attempts = 0;

            while (!isSetup && attempts < 100)
            {
                await BaseScript.Delay(10);
                isSetup = veh.Vehicle.State.Get(StateBagKey.VEHICLE_SETUP) ?? false;
                attempts++;
            }

            bool isServerSpawned = veh.Vehicle.State.Get(StateBagKey.VEH_SPAWNED) ?? false;
            if (!isServerSpawned)
            {
                veh.Vehicle.FuelLevel = 0;
                veh.Vehicle.IsAlarmSet = true;

                if (!veh.Vehicle.IsAlarmSounding)
                    veh.Vehicle.StartAlarm();

                veh.Vehicle.IsEngineRunning = false;
                veh.Vehicle.IsHandbrakeForcedOn = true;
                Notify.Error($"<b>Vehicle is not owned by a player and will not start.</b><br /><br />Please access the store by pressing the [HOME] key, or by the bind you have setup for the Rockstar Social Club.<br /><br />Access to the store can also be found by pressing [M] and opening the PDA at the bottom.", "top-center", true);

                API.PlaySoundFrontend(-1, "ERROR", "HUD_FREEMODE_SOUNDSET", true);
                await BaseScript.Delay(500);
                API.PlaySoundFrontend(-1, "ERROR", "HUD_FREEMODE_SOUNDSET", true);
                await BaseScript.Delay(500);
                API.PlaySoundFrontend(-1, "ERROR", "HUD_FREEMODE_SOUNDSET", true);

                while (Game.PlayerPed.IsInVehicle())
                {
                    if (!veh.Vehicle.IsAlarmSounding)
                    {
                        veh.Vehicle.IsAlarmSet = true;
                        veh.Vehicle.StartAlarm();
                    }

                    await BaseScript.Delay(500);
                }

                return;
            }

            if (!isSetup)
            {
                Vehicle vehicle = veh.Vehicle;
                vehicle.IsPositionFrozen = true;
                vehicle.FuelLevel = 0;

                Game.PlayerPed.Task.WarpOutOfVehicle(vehicle);
                vehicle.Dispose(true);

                Notify.Error("Vehicle failed to be setup. Please try again.");

                return;
            }

            bool setup = currentVehicle.Vehicle.State.Get(StateBagKey.VEH_FUEL_SETUP) ?? false;

            if (!setup)
            {
                Logger.Debug($"VFM: {currentVehicle.Vehicle.Handle}:{setup}");

                bool vehSpawned = currentVehicle.Vehicle.State.Get(StateBagKey.VEH_SPAWNED) ?? false;

                if (vehSpawned)
                {
                    minRandomFuel = 100f;
                    maxRandomFuel = 100f;
                }

                float randomFuel = (float)(minRandomFuel + (maxRandomFuel - minRandomFuel) * (LibUtils.Utility.RANDOM.NextDouble()));
                currentVehicle.Vehicle.State.Set(StateBagKey.VEH_FUEL, randomFuel, true);

                float classMultiplier = 1 / 1600f;
                classMultiplier *= (FuelConsumptionClassMultiplier.ContainsKey(currentVehicle.Vehicle.ClassType) ? FuelConsumptionClassMultiplier[currentVehicle.Vehicle.ClassType] : 1.5f);
                classMultiplier *= 1.0f;
                currentVehicle.Vehicle.State.Set(StateBagKey.VEH_FUEL_MULTIPLIER, classMultiplier, true);

                currentVehicle.Vehicle.State.Set(StateBagKey.VEH_FUEL_SETUP, true, true);
            }

            PluginManager.Instance.AttachTickHandler(OnVehicleFuel);
            PluginManager.Instance.AttachTickHandler(OnVehicleRefuel);
            PluginManager.Instance.AttachTickHandler(CheckFuelPumpDistance);
            PluginManager.Instance.AttachTickHandler(OnManageVehicleBlip);
            PluginManager.Instance.AttachTickHandler(OnDisableDriveBy);
            PluginManager.Instance.AttachTickHandler(OnVehicleInverseTorque);

            uint weapHash = 0;
            GetCurrentPedVehicleWeapon(Game.PlayerPed.Handle, ref weapHash);

            if (weapHash > 0 && veh.Vehicle.ClassType != VehicleClass.Emergency)
                DisableVehicleWeapon(true, weapHash, veh.Vehicle.Handle, Game.PlayerPed.Handle);
        }

        private async Task OnVehicleInverseTorque()
        {
            if (!EnableInverseTorque) return;

            Vehicle vehicle = currentVehicle.Vehicle;
            if (!vehicle.Model.IsCar) return;
            if (vehicle.CurrentGear <= 0) return;

            bool scaleWithGrip = true;
            bool scaleWithGear = true;
            float topMultiplier = 2.0f;
            float deadZone = 4.0f;

            float vehicleTraction = GetVehicleMaxTraction(vehicle.Handle);
            Vector3 forwardVector = vehicle.ForwardVector;
            Vector3 velocity1 = vehicle.Velocity;
            velocity1.Normalize();
            Vector3 velocity1Normalise = velocity1;
            Vector3 upVector = vehicle.UpVector;
            float x = Math.Abs(PositionExtensions.SignedAngle(forwardVector, velocity1Normalise, upVector));
            float torqueMap = 0.0f;
            if (x > deadZone)
                torqueMap = Common.Map(x, 5f, 90f, 1f, (float)(topMultiplier * (scaleWithGrip ? vehicleTraction : 1.0) * (scaleWithGear ? vehicle.CurrentGear : 1.0)), true);
            float torqueMapCopy = torqueMap;
            Vector3 velocity2 = vehicle.Velocity;
            double roundedVelocity = Math.Round(Common.Map(velocity2.Length(), 1f, 5f, 0.0f, 1f, true), 2);
            float engineTorque = (float)(torqueMapCopy * roundedVelocity);
            if (engineTorque > 1.0)
                vehicle.EngineTorqueMultiplier = engineTorque;

        }

        private async Task OnDisableDriveBy()
        {
            double speed = (currentVehicle.Vehicle.Speed * 3.6);

            if (speed >= 1)
            {
                SetPlayerCanDoDriveBy(Game.Player.Handle, false);
            }
            else
            {
                SetPlayerCanDoDriveBy(Game.Player.Handle, true);
            }
        }

        private async Task OnManageVehicleBlip()
        {
            if (Cache.PlayerPed.IsInVehicle())
            {
                Vehicle currentVehicle = Cache.PlayerPed.CurrentVehicle;

                if (currentVehicle.AttachedBlip is not null)
                {
                    if (currentVehicle.AttachedBlip.Exists())
                        currentVehicle.AttachedBlip.Alpha = 0;
                }

            }
        }

        private bool IsNearNormalFuelPump()
        {
            if (!Game.PlayerPed.IsInVehicle()) return false;

            float distanceToPump = (Game.PlayerPed.CurrentVehicle.ClassType == VehicleClass.Boats) ? FUEL_PUMP_RANGE_BOAT : FUEL_PUMP_RANGE;

            return World.GetAllProps().Where(o => FuelPumpModelHashes.Contains((ObjectHash)o.Model.Hash)).Any(o => o.Position.DistanceToSquared(Game.PlayerPed.Position) < Math.Pow(2 * distanceToPump, 2));
        }

        private async Task CheckFuelPumpDistance()
        {
            try
            {
                await BaseScript.Delay(500);
                if (Cache.PlayerPed.IsInVehicle())
                {
                    if (currentVehicle is not null)
                    {
                        if (EletricVehicles.Contains((VehicleHash)currentVehicle.Vehicle.Model.Hash))
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
                ScreenInterface.Draw3DText(currentVehicle.Vehicle.Position, "~w~Press ~b~F2 ~w~to ~y~Refuel ~s~the ~b~Vehicle~n~~w~~b~X ~w~Button on Controller", 40);

                if (ControlHelper.IsControlJustPressed(Control.ReplayStartStopRecordingSecondary, false) && Cache.PlayerPed.IsInVehicle())
                {
                    IsAwaitingServerResponse = true;
                    IsRefueling = true;

                    currentVehicle.Vehicle.IsEngineRunning = false;

                    float fuel = (float)currentVehicle.Vehicle.State.Get(StateBagKey.VEH_FUEL);

                    GenericMessage result = await EventSystem.Request<GenericMessage>("vehicle:refuel:charge", fuel);

                    if (result.Success)
                    {
                        currentVehicle.Vehicle.State.Set(StateBagKey.VEH_FUEL, 100f, true);
                        NotificationManager.GetModule().Success($"Vehicle refueled (${result.Cost})", "bottom-middle");
                    }

                    if (!result.Success)
                    {
                        NotificationManager.GetModule().Warn($"Vehicle <b>not</b> refueled<br />{result.Message}", "bottom-middle");
                    }

                    IsAwaitingServerResponse = false;
                    IsRefueling = false;
                    currentVehicle.Vehicle.IsEngineRunning = true;

                    await BaseScript.Delay(5000);
                }
            }
            else
            {
                await BaseScript.Delay(2000);
            }
        }

        private async Task OnVehicleIsTowing()
        {
            try
            {
                List<Vehicle> vehicles = World.GetAllVehicles().Select(x => x).Where(x => Cache.PlayerPed.IsInRangeOf(x.Position, 20f)).ToList();

                for (int i = 0; i < vehicles.Count; i++)
                {
                    if (Cache.PlayerPed?.CurrentVehicle is not null)
                    {
                        if (Cache.PlayerPed.CurrentVehicle.NetworkId > 0)
                        {
                            Vehicle veh = vehicles[i];

                            if (veh.Exists())
                            {
                                int alpha = Cache.PlayerPed.CurrentVehicle.NetworkId == veh.NetworkId ? 0 : 255;

                                if (veh.AttachedBlip is not null)
                                {
                                    if (veh.AttachedBlip.Alpha != alpha)
                                    {
                                        int blipHandle = API.GetBlipFromEntity(veh.Handle);
                                        API.SetBlipAlpha(blipHandle, alpha);
                                    }
                                }

                                int trailerId = 0;
                                if (API.GetVehicleTrailerVehicle(veh.Handle, ref trailerId))
                                {
                                    int blipId = API.GetBlipFromEntity(trailerId);
                                    if (blipId > 0)
                                        API.SetBlipAlpha(blipId, alpha);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // DON'T FUCKING CARE!
            }
        }

        private async Task OnVehicleFuel()
        {
            if (!Cache.PlayerPed.IsInVehicle())
            {
                GameTimeLeftVehicle = GetGameTimer();
                lastUpdate = -1;
                PluginManager.Instance.DetachTickHandler(OnVehicleFuel);
                PluginManager.Instance.DetachTickHandler(OnVehicleRefuel);
                PluginManager.Instance.DetachTickHandler(CheckFuelPumpDistance);
                PluginManager.Instance.DetachTickHandler(OnVehicleIsTowing);
                PluginManager.Instance.DetachTickHandler(OnDisableDriveBy);
                PluginManager.Instance.DetachTickHandler(OnVehicleInverseTorque);
                IsNearFuelPump = false;
                IsRefueling = false;

                //EventSystem.Send("culling:reset");

                List<Vehicle> vehicles = World.GetAllVehicles().Select(x => x).Where(x => Cache.PlayerPed.IsInRangeOf(x.Position, 20f)).ToList();

                foreach (Vehicle veh in vehicles)
                {
                    if (veh.AttachedBlip is not null)
                    {
                        int blipHandle = API.GetBlipFromEntity(veh.Handle);
                        API.SetBlipAlpha(blipHandle, 255);
                    }

                    bool isAttachedToTrailer = API.IsVehicleAttachedToTrailer(veh.Handle);

                    if (isAttachedToTrailer)
                    {
                        int trailerId = 0;
                        if (API.GetVehicleTrailerVehicle(veh.Handle, ref trailerId))
                        {
                            int blipId = API.GetBlipFromEntity(trailerId);
                            if (blipId > 0)
                                API.SetBlipAlpha(blipId, 255);
                        }
                    }
                }

                return;
            }

            var fuelState = currentVehicle.Vehicle.State.Get(StateBagKey.VEH_FUEL);

            float fuel = 100f;
            if (float.TryParse($"{fuelState}", out float vehFuel))
                fuel = vehFuel;

            float multi = currentVehicle.Vehicle.State.Get(StateBagKey.VEH_FUEL_MULTIPLIER) ?? 1f;

            if (fuel < 2f && !IsRefueling)
            {
                currentVehicle.Vehicle.IsEngineRunning = false;
                currentVehicle.Vehicle.FuelLevel = 0f;

                API.DecorSetFloat(currentVehicle.Vehicle.Handle, DECOR_VEH_FUEL_USE, 0); // LEGACY
                API.DecorSetFloat(currentVehicle.Vehicle.Handle, DECOR_VEH_FUEL, fuel); // LEGACY
            }

            if (ControlHelper.IsControlPressed(Control.VehicleAccelerate, false) && !currentVehicle.Vehicle.IsEngineRunning)
            {
                currentVehicle.Vehicle.IsEngineRunning = true;
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
            float vehicleSpeed = Math.Abs(currentVehicle.Vehicle.Speed);

            if (vehicleSpeed < 4f) // drain fuel while parked
                vehicleSpeed = 30f;

            if (!currentVehicle.Vehicle.IsEngineRunning)
                vehicleSpeed = 0f;

            if (currentVehicle.Vehicle.IsEngineRunning)
            {
                if (fuel < 1f && !IsRefueling)
                {
                    currentVehicle.Vehicle.IsEngineRunning = false;
                    currentVehicle.Vehicle.FuelLevel = 0f;

                    API.DecorSetFloat(currentVehicle.Vehicle.Handle, DECOR_VEH_FUEL_USE, 0); // LEGACY
                    API.DecorSetFloat(currentVehicle.Vehicle.Handle, DECOR_VEH_FUEL, fuel); // LEGACY
                }

                float useAmount = (float)(deltaTime * multi * vehicleSpeed);

                API.DecorSetFloat(currentVehicle.Vehicle.Handle, DECOR_VEH_FUEL_USE, useAmount); // LEGACY
                fuel = Math.Max(0f, fuel - useAmount);
                currentVehicle.Vehicle.FuelLevel = fuel;
                currentVehicle.Vehicle.State.Set(StateBagKey.VEH_FUEL, fuel, true);

                API.DecorSetFloat(currentVehicle.Vehicle.Handle, DECOR_VEH_FUEL, fuel); // LEGACY
            }

            lastUpdate = currentUpdate;

            // Screen.ShowSubtitle($"F: {fuel:0.00000}, FM: {multi:0.00000}, VC: {currentVehicle.ClassType}");

            await BaseScript.Delay(500);
        }
        #endregion

        #region Skylift

        List<ObjectHash> containers = new List<ObjectHash>()
        {
            //ObjectHash.prop_container_01a, // buggy
            ObjectHash.prop_container_01b,
            ObjectHash.prop_container_01c,
            ObjectHash.prop_container_01d,
            ObjectHash.prop_container_01e,
            ObjectHash.prop_container_01f,
            ObjectHash.prop_container_01g,
            ObjectHash.prop_container_01h,
            //ObjectHash.prop_container_01mb,
            //ObjectHash.prop_container_02a,
            //ObjectHash.prop_container_ld2,
            //ObjectHash.prop_container_ld_d,
        };

        Model containerModel = (int)ObjectHash.prop_container_01a;

        internal void InitialiseSkylift(VehicleState veh)
        {
            if (veh.Vehicle.Driver != Cache.PlayerPed) return;

            Logger.Debug($"InitialiseSkylift");

            PluginManager.Instance.AttachTickHandler(OnSkyliftTick);
        }

        private async Task OnSkyliftTick()
        {
            if (!Cache.PlayerPed.IsInVehicle())
            {
                PluginManager.Instance.DetachTickHandler(OnSkyliftTick);
            }

            try
            {

                if (SkyliftSettings.Enabled && ControlHelper.IsControlJustPressed(Control.VehicleGrapplingHook, false))
                {
                    if (this.currentVehicle.AttachedVehicle is null)
                    {
                        Vector3 rot = currentVehicle.Vehicle.Rotation;
                        Vehicle closestVehicle;
                        List<Vehicle> vehicles = World.GetAllVehicles().Where(x => x.IsInRangeOf(currentVehicle.Vehicle.Position, SkyliftSettings.DetectionRadius)).ToList();

                        if (vehicles.Count > 0) // just incase
                            vehicles.Remove(currentVehicle.Vehicle);

                        if (vehicles.Count == 0)
                            return;

                        if (vehicles.Count > 1)
                        {
                            Interface.Notify.Alert($"Too many vehicles nearby.");
                        }
                        else
                        {
                            closestVehicle = vehicles[0];

                            if (closestVehicle.Driver.Exists())
                            {
                                Interface.Notify.Alert($"Vehicle has a driver.");
                                return;
                            }

                            closestVehicle.IsCollisionEnabled = false;

                            EntityBone entityBone = currentVehicle.Vehicle.Bones["bodyshell"];
                            Vector3 spawnOffset = entityBone.Position + new Vector3(0f, -4.1f, -2.5f);

                            containerModel = (int)containers[LibUtils.Utility.RANDOM.Next(containers.Count)];

                            await containerModel.Request(10000);

                            int propNetworkId = await EventSystem.Request<int>("entity:spawn:prop", (uint)containerModel.Hash, spawnOffset.X, spawnOffset.Y, spawnOffset.Z, true, true, true);

                            await BaseScript.Delay(100);

                            Prop container = null;

                            if (propNetworkId > 0)
                            {
                                int entityId = API.NetworkGetEntityFromNetworkId(propNetworkId);
                                await BaseScript.Delay(0);
                                API.NetworkRequestControlOfNetworkId(propNetworkId);
                                await BaseScript.Delay(0);

                                Logger.Debug($"Requested control of Prop: {propNetworkId}:{entityId}:{API.NetworkHasControlOfEntity(entityId)}");

                                while (!API.NetworkHasControlOfEntity(entityId))
                                {
                                    await BaseScript.Delay(100);
                                    API.NetworkRequestControlOfEntity(entityId);
                                }

                                Logger.Debug($"Have control of Prop: {propNetworkId}:{entityId}:{API.NetworkHasControlOfEntity(entityId)}");

                                await BaseScript.Delay(0);
                                container = new CitizenFX.Core.Prop(entityId);
                                await BaseScript.Delay(100);
                            }

                            containerModel.MarkAsNoLongerNeeded();

                            Vector3 dim1 = Vector3.Zero;
                            Vector3 dim2 = Vector3.Zero;

                            if (container is null)
                            {
                                closestVehicle.Model.GetDimensions(out dim1, out dim2);
                                closestVehicle.AttachTo(currentVehicle.Vehicle, new Vector3(0f, (-dim2.Y + dim2.Y) / 4f, -dim2.Z), rot);
                            }
                            else
                            {

                                closestVehicle.FadeOut();

                                closestVehicle.Model.GetDimensions(out dim1, out dim2);

                                Logger.Debug($"Prop: {container.Handle}");

                                container.Heading = currentVehicle.Vehicle.Heading;
                                closestVehicle.Heading = currentVehicle.Vehicle.Heading;

                                container.AttachTo(entityBone, new Vector3(0f, -4.1f, -2.5f));
                                closestVehicle.AttachTo(entityBone, new Vector3(0f, ((-dim2.Y + dim2.Y) / 4f) - 2f, -dim2.Z));

                            }

                            container.IsCollisionEnabled = true;

                            currentVehicle.AttachedProp = container;
                            currentVehicle.AttachedVehicle = new VehicleState(closestVehicle);
                        }

                        Interface.Notify.Info($"Magnet On");

                        await BaseScript.Delay(1000);
                    }
                    else
                    {
                        if (currentVehicle.AttachedProp is not null)
                        {
                            Vector3 pos = currentVehicle.AttachedProp.Position;
                            float groundZ = pos.Z;

                            if (API.GetGroundZFor_3dCoord(pos.X, pos.Y, pos.Z, ref groundZ, false))
                            {
                                currentVehicle.AttachedProp.Detach();
                                currentVehicle.AttachedVehicle.Vehicle.Detach();

                                while ((currentVehicle.AttachedVehicle.Vehicle.Position.Z - groundZ) > 4f)
                                {
                                    await BaseScript.Delay(100);
                                }

                                currentVehicle.AttachedProp.FadeOut();

                                currentVehicle.AttachedVehicle.Vehicle.IsCollisionEnabled = true;
                                currentVehicle.AttachedVehicle.Vehicle.Position = currentVehicle.AttachedVehicle.Vehicle.Position + new Vector3(0f, 0f, 1f);

                                currentVehicle.AttachedProp.Delete();
                                currentVehicle.AttachedProp = null;

                                await BaseScript.Delay(500);
                            }
                            else
                            {
                                currentVehicle.AttachedVehicle.Vehicle.Detach();
                                currentVehicle.AttachedVehicle.Vehicle.IsCollisionEnabled = true;
                            }

                            currentVehicle.AttachedVehicle.Vehicle.Repair();

                            await BaseScript.Delay(500);
                            currentVehicle.AttachedVehicle.Vehicle.IsCollisionProof = false;
                            currentVehicle.AttachedVehicle.Vehicle.FadeIn();

                        }
                        else
                        {
                            currentVehicle.AttachedVehicle.Vehicle.FadeIn();
                            currentVehicle.AttachedVehicle.Vehicle.Detach();
                            currentVehicle.AttachedVehicle.Vehicle.IsCollisionEnabled = true;
                        }

                        Interface.Notify.Info($"Magnet Off");

                        await BaseScript.Delay(1000);

                        currentVehicle.AttachedVehicle = null;

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"OnSkyliftTick -> {ex}");

                if (currentVehicle.AttachedVehicle is not null)
                {
                    if (currentVehicle.AttachedVehicle.Vehicle.Exists())
                        currentVehicle.AttachedVehicle.Vehicle.Delete();

                    currentVehicle.AttachedVehicle = null;
                }

                if (currentVehicle.AttachedProp is not null)
                {
                    if (currentVehicle.AttachedProp.Exists())
                        currentVehicle.AttachedProp.Delete();

                    currentVehicle.AttachedProp = null;
                }
            }
        }
        #endregion

        #region AUTODRIVE

        private int tickCount = 1;
        private int autoDriveTickCount = 1;
        private long lastVehicleControlTime;
        private DrivingStyle driveStyle = (DrivingStyle)786603;

        private const string VEHICLE_AUTO_DRIVE = "curiosity:vehicle:autodrive";

        bool WANDER_ALWAYS = false;
        bool WANDER_TO_WAYPOINT = false;

        long WAIT_TIME = 5;
        int START_SPEED = 100;

        bool ALLOW_CROSSING_MEDIAN = true;
        bool STOP_AT_DESTINATION = true;
        bool STOP_AT_TRAFFIC_LIGHTS = true;
        bool FOLLOW_TRAFFIC = true;
        bool YIELD_TO_PEDESTRIANS = true;
        bool AVOID_PEDESTRIANS = true;
        bool AVOID_VEHICLES = true;
        bool AVOID_OBJECTS = true;
        bool USE_BLINKERS = true;

        bool MANUAL_OVERRIDE_BRAKES = false;
        bool MANUAL_OVERRIDE_GAS = true;
        bool MANUAL_OVERRIDE_STEERING = true;

        bool AUTO_DRIVE_ENABLED = false;
        static Blip WaypointBlip => World.GetWaypointBlip();

        bool IsPlayerDriving => Game.PlayerPed.CurrentVehicle.Driver == Game.PlayerPed;

        public void EnableAutodrive()
        {
            if (!Game.PlayerPed.IsInVehicle())
            {
                NotificationManager.Error($"Must be in a vehicle to enable Auto Drive.");
                return;
            }

            if (Game.PlayerPed.CurrentVehicle.Driver != Game.PlayerPed)
            {
                NotificationManager.Error($"Must the driver of the vehicle enable Auto Drive.");
                return;
            }

            if (World.GetWaypointBlip() == null)
            {
                NotificationManager.Error($"Must set a way point.");
                return;
            }

            DriveToWaypoint();

            Instance.AttachTickHandler(OnVehicleAutoDrive);
        }

        private void DriveToWaypoint()
        {
            Vehicle vehicle = Game.PlayerPed.CurrentVehicle;
            Vector3 position = WaypointBlip.Position;

            VehicleDrivingFlags vehicleDrivingFlags = SetupFlags();

            Game.PlayerPed.Task.DriveTo(vehicle, position, 5f, 30f, (int)vehicleDrivingFlags);
        }

        public void DisableAutodrive()
        {
            Instance.DetachTickHandler(OnVehicleAutoDrive);
            Game.PlayerPed.Task.ClearAll();
        }

        private VehicleDrivingFlags SetupFlags()
        {
            VehicleDrivingFlags vehicleDrivingFlags = (VehicleDrivingFlags)0;
            if (this.ALLOW_CROSSING_MEDIAN)
                vehicleDrivingFlags = (VehicleDrivingFlags)(vehicleDrivingFlags | VehicleDrivingFlags.AllowMedianCrossing);
            if (this.STOP_AT_DESTINATION)
                vehicleDrivingFlags = (VehicleDrivingFlags)(vehicleDrivingFlags | VehicleDrivingFlags.StopAtDestination);
            if (this.STOP_AT_TRAFFIC_LIGHTS)
                vehicleDrivingFlags = (VehicleDrivingFlags)(vehicleDrivingFlags | VehicleDrivingFlags.StopAtTrafficLights);
            if (this.FOLLOW_TRAFFIC)
                vehicleDrivingFlags = (VehicleDrivingFlags)(vehicleDrivingFlags | VehicleDrivingFlags.FollowTraffic);
            if (this.YIELD_TO_PEDESTRIANS)
                vehicleDrivingFlags = (VehicleDrivingFlags)(vehicleDrivingFlags | VehicleDrivingFlags.YieldToPeds);
            if (this.AVOID_PEDESTRIANS)
                vehicleDrivingFlags = (VehicleDrivingFlags)(vehicleDrivingFlags | VehicleDrivingFlags.AvoidPeds);
            if (this.AVOID_VEHICLES)
                vehicleDrivingFlags = (VehicleDrivingFlags)(vehicleDrivingFlags | VehicleDrivingFlags.AvoidVehicles);
            if (this.AVOID_OBJECTS)
                vehicleDrivingFlags = (VehicleDrivingFlags)(vehicleDrivingFlags | VehicleDrivingFlags.AvoidObjects);

            Game.PlayerPed.VehicleDrivingFlags = vehicleDrivingFlags;
            return vehicleDrivingFlags;
        }

        public static Vector3 WaypointPosition
        {
            get
            {
                Blip waypointBlip = WaypointBlip;
                if (Blip.Equals(waypointBlip, (Blip)null))
                    return Vector3.Zero;

                Vector3 position = waypointBlip.Position;
                position.Z = World.GetGroundHeight(position);
                return position;
            }
            set => SetNewWaypoint(value.X, value.Y);
        }

        private bool IsControlPressed(Control control)
        {
            if (Game.IsControlPressed(0, control))
                return true;
            if (Game.IsControlJustPressed(0, control))
                return true;
            if (Game.IsDisabledControlPressed(0, control))
                return true;
            return Game.IsDisabledControlPressed(0, control);
        }

        private bool IsPlayerPressingVehicleControls()
        {
            if (Game.Player.Character.IsInVehicle())
            {
                List<Control> controls = new List<Control>();
                if (this.MANUAL_OVERRIDE_BRAKES)
                    controls.Add(Control.VehicleBrake);
                if (this.MANUAL_OVERRIDE_GAS)
                    controls.Add(Control.VehicleAccelerate);
                if (this.MANUAL_OVERRIDE_STEERING)
                {
                    controls.Add(Control.VehicleMoveLeft);
                    controls.Add(Control.VehicleMoveRight);
                    controls.Add(Control.VehicleMoveUp);
                    controls.Add(Control.VehicleMoveDown);
                }
                controls.Add(Control.VehicleExit);
                foreach (Control control in controls)
                {
                    if (IsControlPressed((Control)control))
                        return true;
                }
            }
            return false;
        }

        private async Task OnVehicleAutoDrive()
        {
            if (!Game.PlayerPed.IsInVehicle())
            {
                Instance.DetachTickHandler(OnVehicleAutoDrive);
                return;
            }

            if (IsPlayerPressingVehicleControls())
            {
                Game.PlayerPed.Task.ClearAll();
                Instance.DetachTickHandler(OnVehicleAutoDrive);
            }

            ++tickCount;

            if (this.tickCount % 25 == 0)
            {
                DriveToWaypoint();
                tickCount = 0;
            }
        }

        #endregion

        internal async Task<dynamic> CreateVehicle(int characterVehicleId, string hash)
        {
            try
            {
                //bool raceActive = Game.Player.State.Get(StateBagKey.PLAYER_RACE_ACTIVE) ?? false;
                //if (raceActive)
                //{
                //    Notify.Error($"You're currently flagged as taking part in an active race.");
                //    return new { success = false };
                //}

                if (!_canSpawn)
                {
                    Notify.Error($"Processing");
                    return new { success = false };
                }

                bool weaponsAreDisabled = PlayerOptionsManager.GetModule().WeaponsDisabled;
                if (weaponsAreDisabled)
                {
                    Notify.Error($"Cannot spawn/change a vehicle currently, you may have a timer active.");
                    return new { success = false };
                }

                bool isWantedByPolice = Game.Player.State.Get(StateBagKey.PLAYER_POLICE_WANTED) ?? false;

                if (Game.Player.WantedLevel > 0)
                {
                    Notify.Error($"Cannot spawn/change a vehicle when wanted.");
                    return new { success = false };
                }

                if (isWantedByPolice)
                {
                    Notify.Error($"Cannot spawn/change a vehicle when wanted.");
                    return new { success = false };
                }

                uint modelHash = (uint)API.GetHashKey(hash);

                if (!API.IsModelInCdimage(modelHash))
                {
                    Notify.Error($"Model '{hash}' is not loaded.");
                    return new { success = false };
                }

                Model vehModel = new Model(hash);

                if (!vehModel.IsValid)
                {
                    Notify.Error($"Model '{hash}' is not valid.");
                    return new { success = false };
                }

                if (!(vehModel?.IsLoaded ?? false))
                {
                    vehModel?.Request();

                    Logger.Debug($"Vehicle model is not loaded");

                    while (!vehModel.IsLoaded)
                    {
                        await BaseScript.Delay(0);
                    }
                }

                if (!vehModel.IsVehicle)
                {
                    vehModel.MarkAsNoLongerNeeded();
                    Notify.Error($"Model '{hash}' is not a vehicle.");
                    return new { success = false };
                }

                Logger.Debug($"Vehicle model is now loaded");

                Vector3 charPos = Game.PlayerPed.Position;
                Vector3 spawnPos = Vector3.Zero;
                float spawnHeading = 0f;

                Vector3 spawnRoad = Vector3.Zero;

                API.GetClosestVehicleNodeWithHeading(charPos.X, charPos.Y, charPos.Z, ref spawnPos, ref spawnHeading, 1, 3f, 0);
                API.GetRoadSidePointWithHeading(spawnPos.X, spawnPos.Y, spawnPos.Z, spawnHeading, ref spawnRoad);

                float distance = vehModel.GetDimensions().Y;

                VehicleItem vehicleItem = await EventSystem.Request<VehicleItem>("garage:get:vehicle", characterVehicleId, spawnRoad.X, spawnRoad.Y, spawnRoad.Z, spawnHeading, distance, (uint)vehModel.Hash);
                await BaseScript.Delay(0);

                if (API.IsAnyVehicleNearPoint(vehicleItem.X, vehicleItem.Y, vehicleItem.Z, distance) && vehicleItem.SpawnTypeId != SpawnType.Vehicle)
                {
                    Notify.Info("Either you're currently in a vehicle, or your current location is blocked by another vehicle.");
                    vehModel.MarkAsNoLongerNeeded();
                    return new { success = false };
                }


                if (vehicleItem is null)
                {
                    vehModel.MarkAsNoLongerNeeded();
                    return new { success = false };
                }

                if (!string.IsNullOrEmpty(vehicleItem.Message))
                {
                    Notify.Error(vehicleItem.Message, playSound: true);
                    vehModel.MarkAsNoLongerNeeded();
                    return new { success = false };
                }

                float velocity = 0;

                Vector3 returnedSpawnPosition = new Vector3(vehicleItem.X, vehicleItem.Y, vehicleItem.Z);
                API.ClearAreaOfEverything(returnedSpawnPosition.X, returnedSpawnPosition.Y, returnedSpawnPosition.Z, 4f, false, false, false, false);
                Vector3 postionSpawn = returnedSpawnPosition; // create vehicles in a controlled location
                postionSpawn.Z = postionSpawn.Z - 50f;

                if (vehicleItem.SpawnTypeId == SpawnType.Vehicle)
                    postionSpawn.Z = postionSpawn.Z + 0.5f;

                float heading = vehicleItem.Heading;

                if (Game.PlayerPed.IsInVehicle())
                {
                    velocity = Game.PlayerPed.CurrentVehicle.Speed;
                    postionSpawn = Game.PlayerPed.CurrentVehicle.Position;
                    heading = Game.PlayerPed.CurrentVehicle.Heading;
                }

                Vehicle previousVehicle = null;

                if (Cache.PersonalVehicle is not null && vehicleItem.SpawnTypeId == SpawnType.Vehicle)
                    previousVehicle = Cache.PersonalVehicle.Vehicle;

                if (Cache.PersonalBoat is not null && vehicleItem.SpawnTypeId == SpawnType.Boat)
                    previousVehicle = Cache.PersonalBoat.Vehicle;

                if (Cache.PersonalHelicopter is not null && vehicleItem.SpawnTypeId == SpawnType.Helicopter)
                    previousVehicle = Cache.PersonalHelicopter.Vehicle;

                if (Cache.PersonalPlane is not null && vehicleItem.SpawnTypeId == SpawnType.Boat)
                    previousVehicle = Cache.PersonalBoat.Vehicle;

                if (Cache.PersonalTrailer is not null && vehicleItem.SpawnTypeId == SpawnType.Trailer)
                    previousVehicle = Cache.PersonalTrailer.Vehicle;

                if (previousVehicle is not null)
                {
                    previousVehicle.Dispose();
                }

                Vehicle vehicle = null;

                vehicle = await World.CreateVehicle(vehModel, postionSpawn, heading);

                if (velocity > 0f)
                {
                    RequestNamedPtfxAsset("scr_powerplay");
                    SetPtfxAssetNextCall("scr_powerplay");
                    int fxHandle = StartParticleFxLoopedOnEntity("scr_powerplay_beast_appear", vehicle.Handle, 0f, 0f, 0f, 0f, 0f, 0f, 2.0f, false, false, false);
                    float r = GetRandomFloatInRange(0f, 1f);
                    float g = GetRandomFloatInRange(0f, 1f);
                    float b = GetRandomFloatInRange(0f, 1f);
                    SetParticleFxLoopedColour(fxHandle, r, g, b, false);

                    //Utils.ParticleEffectsAssetNetworked particleEffectsAssetNetworked = new Utils.ParticleEffectsAssetNetworked($"scr_powerplay");
                    //particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_powerplay_beast_appear", previousVehicle.Position, scale: 4f);

                }

                API.NetworkRequestControlOfEntity(vehicle.Handle);
                vehicle.IsPersistent = true;
                vehicle.ApplyVehicleModsDelayed(vehicleItem.VehicleInfo, 1000);
                vehicle.Repair();
                vehModel.MarkAsNoLongerNeeded();

                // setup vehicle on the server
                API.SetNetworkIdExistsOnAllMachines(vehicle.NetworkId, true);
                API.SetNetworkIdCanMigrate(vehicle.NetworkId, true);
                API.SetVehicleHasBeenOwnedByPlayer(vehicle.Handle, true);

                VehicleSetupServerSide(vehicle, (int)vehicleItem.SpawnTypeId, vehicleItem.CharacterVehicleId);

                vehicle.PlaceOnGround();

                vehicle.RadioStation = RadioStation.RadioOff;

                vehicle.Mods.LicensePlate = vehicleItem.VehicleInfo.plateText;

                if (vehicleItem.SpawnTypeId == SpawnType.Vehicle)
                {
                    Cache.PersonalVehicle = null;
                    Cache.PersonalVehicle = new State.VehicleState(vehicle);
                    Cache.PlayerPed.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                    Cache.Player.User.SendEvent("vehicle:log:player", vehicle.NetworkId);

                    vehicle.IsEngineRunning = velocity > 0f;
                    vehicle.Speed = velocity;
                }

                if (vehicleItem.SpawnTypeId == SpawnType.Plane)
                {
                    Cache.PersonalPlane = null;
                    Cache.PersonalPlane = new State.VehicleState(vehicle);
                    Cache.Player.User.SendEvent("vehicle:log:player:plane", vehicle.NetworkId);
                }

                if (vehicleItem.SpawnTypeId == SpawnType.Boat)
                {
                    Cache.PersonalBoat = null;
                    Cache.PersonalBoat = new State.VehicleState(vehicle);
                    Cache.Player.User.SendEvent("vehicle:log:player:boat", vehicle.NetworkId);
                }

                if (vehicleItem.SpawnTypeId == SpawnType.Helicopter)
                {
                    Cache.PersonalHelicopter = null;
                    Cache.PersonalHelicopter = new State.VehicleState(vehicle);
                    Cache.Player.User.SendEvent("vehicle:log:player:helicopter", vehicle.NetworkId);
                }

                if (vehicleItem.SpawnTypeId == SpawnType.Trailer)
                {
                    Cache.PersonalTrailer = null;
                    Cache.Player.User.SendEvent("vehicle:log:player:trailer", vehicle.NetworkId);
                    Cache.PersonalTrailer = new State.VehicleState(vehicle);
                }

                Blip blip = vehicle.CreateBlip(vehicleItem.SpawnTypeId);

                //if (vehicleItem.SpawnTypeId != SpawnType.Trailer)
                //    API.SetVehicleExclusiveDriver_2(vehicle.Handle, Game.PlayerPed.Handle, 1);

                vehicle.State.Set(StateBagKey.VEH_SPAWN_TYPE, vehicleItem.SpawnTypeId, false);
                vehicle.State.Set(StateBagKey.BLIP_ID, blip.Handle, false);

                if (Game.PlayerPed.Position.Distance(returnedSpawnPosition) > 50)
                    SetNewWaypoint(returnedSpawnPosition.X, returnedSpawnPosition.Y);

                vehicle.IsPositionFrozen = false;
                vehicle.IsCollisionEnabled = true;
                vehicle.FadeIn();

                if (API.DecorIsRegisteredAsType("Player_Vehicle", 3))
                {
                    API.DecorSetInt(vehicle.Handle, "Player_Vehicle", -1);
                }

                if (velocity == 0f)
                {
                    vehicle.Position = returnedSpawnPosition;
                    vehicle.Heading = vehicleItem.Heading;

                    vehicle.PlaceOnGround();
                }

                API.SetVehicleAutoRepairDisabled(vehicle.Handle, true);

                Notify.Success("Vehicle has been requested successfully, please follow the waypoint on your map.");

                return new { success = true };
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Oh well....");
                Notify.Error("FiveM fucked something up");
                return new { success = false };
            }
        }

        async void VehicleSetupServerSide(Vehicle vehicle, int spawnTypeId, int characterVehicleId)
        {
            _canSpawn = false;

            Logger.Debug($"Check Server Setup");
            int networkId = vehicle.NetworkId;

            bool setupCompleted = await EventSystem.Request<bool>("garage:set:vehicle", networkId, spawnTypeId, characterVehicleId);
            int attempts = 0;
            Logger.Debug($"IsSetupCompleted: {setupCompleted}");

            if (setupCompleted)
                vehicle.State.Set(StateBagKey.VEH_SPAWNED, true, true);

            while (!setupCompleted && attempts < 100)
            {
                if (!vehicle.Exists()) break;

                bool isServerConfirmed = vehicle.State.Get(StateBagKey.VEH_SPAWNED) ?? false;
                await BaseScript.Delay(100);
                setupCompleted = await EventSystem.Request<bool>("garage:set:vehicle", networkId, spawnTypeId, characterVehicleId) && isServerConfirmed;
                attempts++;
                Logger.Debug($"Check Server Setup ({isServerConfirmed}: Attempt #{attempts}");
            }

            if (!setupCompleted)
            {
                Logger.Debug($"DELETE VEHICLE");
                vehicle.Dispose();
            }

            bool serverSpawned = vehicle.State.Get(StateBagKey.VEH_SPAWNED) ?? false;
            Logger.Debug($"serverSpawned: {serverSpawned}");

            vehicle.State.Set(StateBagKey.VEHICLE_SETUP, true, true);

            _canSpawn = true;
        }
    }
}

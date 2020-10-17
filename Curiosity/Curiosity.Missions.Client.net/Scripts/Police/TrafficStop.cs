using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Missions.Client.Utils;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.Scripts.Police
{
    /*
     * Current Bugs;
     * Lost Vehicle Hud Sign needs to hide
     * Ped cannot get back into vehicle
     * Ped doesn't leave group
     * 
     */


    class TrafficStop
    {
        static PluginManager PluginInstance => PluginManager.Instance;
        static bool IsScenarioPlaying = false;

        static float DistanceToCheck = 20.0f;

        static bool IsAwaitingPullover = true;
        static bool IsConductingPullover = false;
        static bool IsCooldownActive = false;

        static Vehicle _vehicle;
        static Ped _ped;
        static int _vehicleNetworkId;

        static string currentMessage = string.Empty;

        private static string loadingMessage = string.Empty;

        // states

        public static void Setup()
        {
            loadingMessage = string.Empty; // Because it showed some wierd shit.

            PluginInstance.RegisterTickHandler(OnTrafficStopTask);
            PluginInstance.RegisterTickHandler(OnEmoteCheck);
            PluginInstance.RegisterTickHandler(OnShowLoading);
            PluginInstance.RegisterTickHandler(OnDeveloperData);

            PluginInstance.RegisterEventHandler("curiosity:interaction:vehicle:released", new Action<int>(OnVehicleHasBeenReleased));

            Screen.ShowNotification("~b~Traffic Stops~s~: ~g~Enabled");
        }

        public static void Dispose()
        {
            PluginInstance.DeregisterTickHandler(OnTrafficStopTask);
            PluginInstance.DeregisterTickHandler(OnEmoteCheck);
            PluginInstance.DeregisterTickHandler(OnShowLoading);

            PluginInstance.DeregisterTickHandler(OnDeveloperData);
            IsConductingPullover = false;

            loadingMessage = string.Empty;

            Screen.ShowNotification("~b~Traffic Stops~s~: ~r~Disabled");
            PluginManager.TriggerEvent("curiosity:Client:Police:TrafficStops", false);
        }

        static async Task OnEmoteCheck()
        {
            await PluginManager.Delay(0);
            if (IsScenarioPlaying)
            {
                if (
                    Game.IsControlPressed(0, Control.MoveDown)
                    || Game.IsControlPressed(0, Control.MoveUp)
                    || Game.IsControlPressed(0, Control.MoveLeft)
                    || Game.IsControlPressed(0, Control.MoveRight)
                    )
                {
                    Game.PlayerPed.Task.ClearAll();
                    IsScenarioPlaying = false;
                }
            }
        }

        static async Task OnDeveloperData()
        {
            await Task.FromResult(0);
            if (PluginManager.DeveloperVehUiEnabled)
            {
                string isConductingPullOver = IsConductingPullover ? "~g~" : "~r~";
                string isCooldownActive = IsCooldownActive ? "~g~" : "~r~";
                string isAwaitingPullover = IsAwaitingPullover ? "~g~" : "~r~";

                Screen.ShowSubtitle($"{isConductingPullOver}isConductingPullover, {isCooldownActive}IsCooldownActive, {isAwaitingPullover}AwaitingPullover\n~s~{_vehicleNetworkId}");

                try
                {
                    PluginManager.CurrentVehicle.DrawEntityHit(DistanceToCheck);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            else
            {
                await PluginManager.Delay(1000);
            }
        }

        static async Task OnTrafficStopTask()
        {
            try
            {
                if (IsConductingPullover)
                {
                    await BaseScript.Delay(5000);
                    return;
                }

                if (Game.PlayerPed.IsInVehicle() && PluginManager.CurrentVehicle != null)
                {
                    if (Game.PlayerPed.CurrentVehicle != PluginManager.CurrentVehicle)
                    {
                        Screen.ShowNotification($"~b~Traffic Stop: ~w~Must be using a Personal Vehicle from the garage.");
                        await BaseScript.Delay(60000);
                        return;
                    }

                    if (Game.PlayerPed.CurrentVehicle.ClassType == VehicleClass.Emergency)
                    {
                        if (IsCooldownActive)
                        {
                            return;
                        }

                        Vehicle targetVehicle = PluginManager.CurrentVehicle.GetVehicleInFront(DistanceToCheck);
                        // Safe Checks
                        if (targetVehicle == null) return;
                        if (targetVehicle.Driver.IsPlayer) return;
                        if (targetVehicle.Driver == null) return;
                        if (targetVehicle.Driver.IsDead) return;

                        if (Decorators.GetBoolean(targetVehicle.Handle, PluginManager.DECOR_VEHICLE_HAS_BEEN_TRAFFIC_STOPPED))
                        {
                            Screen.DisplayHelpTextThisFrame($"~b~Traffic Stops: ~r~This vehicle has already been stopped.");
                            return;
                        }

                        if (Decorators.GetBoolean(targetVehicle.Handle, PluginManager.DECOR_VEHICLE_IGNORE))
                        {
                            Screen.DisplayHelpTextThisFrame($"~b~Traffic Stops: ~r~This vehicle is being ignored.");
                            return;
                        }

                        loadingMessage = "Vehicle: Detected";

                        long gameTime = GetGameTimer();
                        while ((API.GetGameTimer() - gameTime) < 5000)
                        {
                            if (PluginManager.CurrentVehicle.GetVehicleInFront(DistanceToCheck) == null)
                            {
                                loadingMessage = "Vehicle: Lost";
                                await BaseScript.Delay(2000);
                                loadingMessage = string.Empty;
                                return;
                            }

                            await BaseScript.Delay(0);
                        }

                        bool hasBeenPulledOver = Decorators.GetBoolean(targetVehicle.Handle, PluginManager.DECOR_VEHICLE_HAS_BEEN_TRAFFIC_STOPPED);

                        if (hasBeenPulledOver)
                        {
                            Screen.DisplayHelpTextThisFrame($"~b~Traffic Stops: ~r~You have already pulled over this vehicle.");
                            loadingMessage = string.Empty;
                            return;
                        }

                        int playerIdOnDetectedVehicle = Decorators.GetInteger(targetVehicle.Handle, PluginManager.DECOR_VEHICLE_DETECTED_BY);

                        if (playerIdOnDetectedVehicle != Game.Player.ServerId)
                        {
                            Screen.DisplayHelpTextThisFrame($"~b~Traffic Stops: ~r~Sorry this vehicle was marked by another player.");
                            loadingMessage = string.Empty;
                        };

                        Decorators.Set(targetVehicle.Handle, PluginManager.DECOR_VEHICLE_DETECTED_BY, Game.Player.ServerId);

                        // request network control
                        Wrappers.Helpers.RequestControlOfEnt(targetVehicle);

                        bool awaitingPullover = true;
                        if (targetVehicle == PluginManager.CurrentVehicle.GetVehicleInFront(DistanceToCheck))
                        {
                            loadingMessage = "Awaiting Confirmation";

                            while (awaitingPullover && !IsConductingPullover)
                            {
                                await BaseScript.Delay(0);
                                Screen.DisplayHelpTextThisFrame($"Press ~INPUT_PICKUP~ to initiate a ~b~Traffic Stop~w~.\nPress ~INPUT_COVER~ to cancel.");


                                if (targetVehicle.AttachedBlip == null)
                                {
                                    targetVehicle.AttachBlip();
                                    targetVehicle.AttachedBlip.Sprite = BlipSprite.Standard;
                                    targetVehicle.AttachedBlip.Color = BlipColor.TrevorOrange;
                                    targetVehicle.AttachedBlip.IsFlashing = true;
                                }

                                if (Game.IsControlJustPressed(0, Control.Pickup))
                                {
                                    loadingMessage = string.Empty;
                                    targetVehicle.AttachedBlip.IsFlashing = false;
                                    awaitingPullover = false;
                                    IsConductingPullover = true;

                                    _ped = targetVehicle.Driver;
                                    _vehicle = targetVehicle;

                                    _vehicleNetworkId = targetVehicle.NetworkId;

                                    targetVehicle.AttachedBlip.Color = BlipColor.MichaelBlue;

                                    Decorators.Set(targetVehicle.Handle, PluginManager.DECOR_VEHICLE_HAS_BEEN_TRAFFIC_STOPPED, true);

                                    VehicleCreators.CreateVehicles.TrafficStop(targetVehicle);
                                    return;
                                }

                                if (Game.IsControlJustPressed(0, Control.Cover))
                                {
                                    loadingMessage = string.Empty;
                                    awaitingPullover = false;

                                    if (targetVehicle.AttachedBlip != null)
                                        targetVehicle.AttachedBlip.Delete();

                                    Decorators.Set(targetVehicle.Handle, PluginManager.DECOR_VEHICLE_IGNORE, true);
                                    return;
                                }

                                if (targetVehicle.Position.Distance(PluginManager.CurrentVehicle.Position) > 40f)
                                {
                                    loadingMessage = string.Empty;

                                    awaitingPullover = false;

                                    if (targetVehicle.AttachedBlip != null)
                                        targetVehicle.AttachedBlip.Delete();
                                }
                            }
                            API.SetUserRadioControlEnabled(false);
                            loadingMessage = string.Empty;
                        }
                        else
                        {
                            loadingMessage = string.Empty;
                        }
                    }
                    else
                    {
                        if (Game.PlayerPed.IsInVehicle())
                        {
                            Screen.ShowNotification($"~b~Traffic Stop: ~w~Current vehicle type of ~r~{Game.PlayerPed.CurrentVehicle.ClassType} ~w~is not valid.");
                            await PluginManager.Delay(2000);
                            PluginManager.TriggerEvent("curiosity:Client:Police:TrafficStops", false);
                            Dispose();
                            loadingMessage = string.Empty;
                        }
                        loadingMessage = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnTrafficStopTask -> {ex}");
                loadingMessage = string.Empty;
            }
        }

        // RESET
        static async Task OnShowLoading()
        {
            await BaseScript.Delay(0);
            if (!string.IsNullOrEmpty(loadingMessage))
            {
                if (currentMessage != loadingMessage)
                {
                    currentMessage = loadingMessage;
                    Screen.LoadingPrompt.Show(currentMessage, LoadingSpinnerType.SocialClubSaving);
                }
            }
            else
            {
                Screen.LoadingPrompt.Hide();
            }
        }

        static async Task OnCooldownTask()
        {
            if (IsCooldownActive)
            {
                Log.Info($"OnCooldownTask -> Task Already Active");
                return;
            }

            IsCooldownActive = true;
            int timer = 60000;
            long gameTimer = API.GetGameTimer();
            int wait = 60;

            while ((API.GetGameTimer() - gameTimer) < timer)
            {
                loadingMessage = $"Traffic Stop: ~g~Cooldown Active: {wait}s";
                wait--;
                await BaseScript.Delay(1000);
            }

            loadingMessage = "Cooldown Clearing";

            await BaseScript.Delay(3000);

            IsCooldownActive = false;
            IsConductingPullover = false;

            _vehicleNetworkId = 0;
            _vehicle = null;
            loadingMessage = string.Empty;

            PluginInstance.DeregisterTickHandler(OnCooldownTask);
        }

        private static void OnVehicleHasBeenReleased(int networkId)
        {
            Log.Info($"OnVehicleHasBeenReleased -> Traffic Stop reset called");

            List<Vehicle> vehicles = World.GetAllVehicles().Where(x => Decorators.GetBoolean(x.Handle, PluginManager.DECOR_VEHICLE_HAS_BEEN_TRAFFIC_STOPPED)).ToList();

            if (vehicles.Count > 0)
            {
                if (!IsCooldownActive)
                    PluginInstance.RegisterTickHandler(OnCooldownTask);
            }
            else if (!VehicleExsits())
            {
                if (!IsCooldownActive)
                    PluginInstance.RegisterTickHandler(OnCooldownTask);
            }
            else
            {
                PluginInstance.RegisterTickHandler(OnCooldownTask);
            }
        }

        private static bool VehicleExsits()
        {
            if (_vehicle != null)
            {
                return _vehicle.Exists() && _vehicle.IsAlive;
            }
            return false;
        }
    }
}

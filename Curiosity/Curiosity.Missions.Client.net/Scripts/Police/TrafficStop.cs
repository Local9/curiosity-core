using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Missions.Client.net.DataClasses;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Missions.Client.net.Wrappers;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Enums.Patrol;
using Curiosity.Shared.Client.net.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.net.Scripts.Police
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
        static Client client = Client.GetInstance();
        static bool IsScenarioPlaying = false;

        static float DistanceToCheck = 20.0f;

        static bool AwaitingPullover = true;
        static bool isConductingPullover = false;
        static bool IsCooldownActive = false;

        static Vehicle _vehicle;
        static Ped _ped;

        static string currentMessage = string.Empty;

        private static string loadingMessage = string.Empty;

        // states

        public static void Setup()
        {
            loadingMessage = string.Empty; // Because it showed some wierd shit.

            int policeXp = Classes.PlayerClient.ClientInformation.playerInfo.Skills["policexp"].Value;
            int knowledge = Classes.PlayerClient.ClientInformation.playerInfo.Skills["knowledge"].Value;
            if (policeXp >= 2500 && knowledge >= 1000)
            {
                client.RegisterTickHandler(OnTrafficStopTask);
                client.RegisterTickHandler(OnEmoteCheck);
                client.RegisterTickHandler(OnShowLoading);
                client.RegisterTickHandler(OnDeveloperData);

                client.RegisterEventHandler("curiosity:interaction:vehicle:towed", new Action<int>(OnVehicleHasBeenTowed));

                Screen.ShowNotification("~b~Traffic Stops~s~: ~g~Enabled");
            }
            else
            {
                Screen.ShowNotification($"~b~Traffic Stops~s~: ~o~Missing Req\n~b~Remaining:\n  ~b~PoliceXp: ~w~{2500 - policeXp}\n  ~b~Knowledge: ~w~{1000 - knowledge}");
            }
        }

        public static void Dispose()
        {
            client.DeregisterTickHandler(OnTrafficStopTask);
            client.DeregisterTickHandler(OnEmoteCheck);
            client.DeregisterTickHandler(OnShowLoading);

            client.DeregisterTickHandler(OnDeveloperData);
            isConductingPullover = false;

            loadingMessage = string.Empty;

            Screen.ShowNotification("~b~Traffic Stops~s~: ~r~Disabled");
            Client.TriggerEvent("curiosity:Client:Police:TrafficStops", false);
        }

        static async Task OnEmoteCheck()
        {
            await Client.Delay(0);
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
            if (Client.DeveloperVehUiEnabled)
            {
                try
                {
                    Client.CurrentVehicle.DrawEntityHit(DistanceToCheck);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            else
            {
                await Client.Delay(1000);
            }
        }

        static async Task OnTrafficStopTask()
        {
            try
            {
                await BaseScript.Delay(0);

                if (isConductingPullover)
                {
                    await BaseScript.Delay(60000);
                    return;
                }

                if (Game.PlayerPed.IsInVehicle() && Client.CurrentVehicle != null)
                {
                    if (Game.PlayerPed.CurrentVehicle != Client.CurrentVehicle)
                    {
                        Screen.ShowNotification($"~b~Traffic Stop: ~w~Must be using a Personal Vehicle from the garage.");
                        await BaseScript.Delay(60000);
                        return;
                    }

                    if (Game.PlayerPed.CurrentVehicle.ClassType == VehicleClass.Emergency)
                    {
                        Vehicle targetVehicle = Client.CurrentVehicle.GetVehicleInFront(DistanceToCheck);
                        // Safe Checks
                        if (targetVehicle == null) return;
                        if (targetVehicle.Driver.IsPlayer) return;
                        if (targetVehicle.Driver == null) return;
                        if (targetVehicle.Driver.IsDead) return;

                        if (DecorExistOn(targetVehicle.Handle, Client.VEHICLE_HAS_BEEN_TRAFFIC_STOPPED) || DecorExistOn(targetVehicle.Handle, Client.VEHICLE_IGNORE))
                        {
                            if (DecorGetBool(targetVehicle.Handle, Client.VEHICLE_HAS_BEEN_TRAFFIC_STOPPED))
                            {
                                Screen.DisplayHelpTextThisFrame($"~b~Traffic Stops: ~r~This vehicle has already been stopped.");
                                return;
                            }

                            if (DecorGetBool(targetVehicle.Handle, Client.VEHICLE_IGNORE))
                            {
                                Screen.DisplayHelpTextThisFrame($"~b~Traffic Stops: ~r~This vehicle is being ignored.");
                                return;
                            }
                        }

                        loadingMessage = "Vehicle: Detected";

                        long gameTime = GetGameTimer();
                        while ((API.GetGameTimer() - gameTime) < 5000)
                        {
                            if (Client.CurrentVehicle.GetVehicleInFront(DistanceToCheck) == null)
                            {
                                loadingMessage = "Vehicle: Lost";
                                await BaseScript.Delay(2000);
                                loadingMessage = string.Empty;
                                return;
                            }

                            await BaseScript.Delay(0);
                        }

                        if (IsCooldownActive)
                        {
                            targetVehicle = null;
                            Wrappers.Helpers.ShowSimpleNotification("~b~Traffic Stops: ~r~Cooldown Active");
                            return;
                        }

                        bool hasBeenPulledOver = DecorGetBool(targetVehicle.Handle, Client.VEHICLE_HAS_BEEN_TRAFFIC_STOPPED);

                        if (hasBeenPulledOver)
                        {
                            Screen.DisplayHelpTextThisFrame($"~b~Traffic Stops: ~r~You have already pulled over this vehicle.");
                            return;
                        }

                        if (DecorExistOn(targetVehicle.Handle, Client.VEHICLE_DETECTED_BY))
                        {
                            int playerIdOnDetectedVehicle = DecorGetInt(targetVehicle.Handle, Client.VEHICLE_DETECTED_BY);

                            if (playerIdOnDetectedVehicle != Game.Player.ServerId) return; // No point showing anything.
                        }

                        DecorSetInt(targetVehicle.Handle, Client.VEHICLE_DETECTED_BY, Game.Player.ServerId);

                        // request network control
                        NetworkRequestControlOfEntity(targetVehicle.Handle);
                        SetNetworkIdCanMigrate(targetVehicle.NetworkId, true);
                        NetworkRegisterEntityAsNetworked(targetVehicle.NetworkId);
                        SetNetworkIdExistsOnAllMachines(targetVehicle.NetworkId, true);

                        if (!IsEntityAMissionEntity(targetVehicle.Handle))
                            SetEntityAsMissionEntity(targetVehicle.Handle, true, true);

                        bool awaitingPullover = true;
                        if (targetVehicle == Client.CurrentVehicle.GetVehicleInFront(DistanceToCheck))
                        {
                            loadingMessage = "Awaiting Confirmation";

                            while (awaitingPullover && !isConductingPullover)
                            {
                                API.SetUserRadioControlEnabled(false);

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
                                    targetVehicle.AttachedBlip.IsFlashing = false;
                                    awaitingPullover = false;
                                    isConductingPullover = true;

                                    _ped = targetVehicle.Driver;
                                    _vehicle = targetVehicle;

                                    Scripts.VehicleCreators.CreateVehicles.TrafficStop(targetVehicle);
                                    loadingMessage = string.Empty;
                                }

                                if (Game.IsControlJustPressed(0, Control.Cover))
                                {
                                    awaitingPullover = false;

                                    if (targetVehicle.AttachedBlip != null)
                                        targetVehicle.AttachedBlip.Delete();

                                    DecorSetBool(targetVehicle.Handle, Client.VEHICLE_IGNORE, true);
                                    loadingMessage = string.Empty;
                                }

                                if (targetVehicle.Position.Distance(Client.CurrentVehicle.Position) > 40f)
                                {
                                    awaitingPullover = false;

                                    if (targetVehicle.AttachedBlip != null)
                                        targetVehicle.AttachedBlip.Delete();

                                    loadingMessage = string.Empty;
                                }
                            }
                            API.SetUserRadioControlEnabled(false);
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
                            await Client.Delay(2000);
                            Client.TriggerEvent("curiosity:Client:Police:TrafficStops", false);
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
            IsCooldownActive = true;
            int timer = 60;
            while (timer > 0)
            {
                loadingMessage = $"Traffic Stop: Active in {timer}s";
                await Client.Delay(1000);
                timer--;
            }
            loadingMessage = string.Empty;
            IsCooldownActive = false;
            isConductingPullover = false;
            client.DeregisterTickHandler(OnCooldownTask);
        }

        private static void OnVehicleHasBeenTowed(int handle)
        {
            API.SetUserRadioControlEnabled(true);

            if (_vehicle.Handle == handle)
            {
                client.RegisterTickHandler(OnCooldownTask);
            }
        }
    }
}

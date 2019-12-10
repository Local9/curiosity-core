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
    class TrafficStop
    {
        public const string VEHICLE_HAS_BEEN_STOPPED = "curiosity::VehicleStopped";
        public const string IS_TRAFFIC_STOPPED_PED = "curiosity::PedIsTrafficStopped";
        private const string NPC_VEHICLE_IGNORE = "NPC_VEHICLE_IGNORE";
        static Client client = Client.GetInstance();
        static bool IsScenarioPlaying = false;

        static float DistanceToCheck = 20.0f;

        static bool AwaitingPullover = true;
        static bool IsConductingPullover = false;
        static bool IsCooldownActive = false;

        // states

        public static void Setup()
        {
            int policeXp = Classes.PlayerClient.ClientInformation.playerInfo.Skills["policexp"].Value;
            int knowledge = Classes.PlayerClient.ClientInformation.playerInfo.Skills["knowledge"].Value;
            if (policeXp >= 2500 && knowledge >= 1000)
            {
                client.RegisterTickHandler(OnTrafficStopTask);
                client.RegisterTickHandler(OnEmoteCheck);

                client.RegisterTickHandler(OnDeveloperData);

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

            client.DeregisterTickHandler(OnDeveloperData);
            IsConductingPullover = false;

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
            if (Client.DeveloperUiEnabled)
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

                if (IsConductingPullover)
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

                        if (DecorExistOn(targetVehicle.Handle, VEHICLE_HAS_BEEN_STOPPED) || DecorExistOn(targetVehicle.Handle, NPC_VEHICLE_IGNORE))
                        {
                            if (DecorGetBool(targetVehicle.Handle, VEHICLE_HAS_BEEN_STOPPED))
                            {
                                Screen.DisplayHelpTextThisFrame($"~b~Traffic Stops: ~r~This vehicle has already been stopped.");
                                return;
                            }

                            if (DecorGetBool(targetVehicle.Handle, NPC_VEHICLE_IGNORE))
                            {
                                Screen.DisplayHelpTextThisFrame($"~b~Traffic Stops: ~r~This vehicle is being ignored.");
                                return;
                            }
                        }

                        Screen.LoadingPrompt.Show("Vehicle: Detected");

                        long gameTime = GetGameTimer();
                        while ((API.GetGameTimer() - gameTime) < 5000)
                        {
                            if (Client.CurrentVehicle.GetVehicleInFront(DistanceToCheck) == null)
                            {
                                Screen.LoadingPrompt.Show("Vehicle: Lost");
                                await BaseScript.Delay(2000);
                                Screen.LoadingPrompt.Hide();
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

                        bool hasBeenPulledOver = DecorGetBool(targetVehicle.Handle, VEHICLE_HAS_BEEN_STOPPED);

                        if (hasBeenPulledOver)
                        {
                            Screen.DisplayHelpTextThisFrame($"~b~Traffic Stops: ~r~You have already pulled over this vehicle.");
                            return;
                        }

                        bool awaitingPullover = true;
                        if (targetVehicle == Client.CurrentVehicle.GetVehicleInFront(DistanceToCheck))
                        {
                            Screen.LoadingPrompt.Show("Awaiting Confirmation", LoadingSpinnerType.SocialClubSaving);

                            while (awaitingPullover)
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
                                    Screen.LoadingPrompt.Hide();
                                }

                                if (Game.IsControlJustPressed(0, Control.Cover))
                                {
                                    awaitingPullover = false;

                                    if (targetVehicle.AttachedBlip != null)
                                        targetVehicle.AttachedBlip.Delete();

                                    DecorSetBool(targetVehicle.Handle, NPC_VEHICLE_IGNORE, true);
                                    Screen.LoadingPrompt.Hide();
                                }

                                if (targetVehicle.Position.Distance(Client.CurrentVehicle.Position) > 40f)
                                {
                                    awaitingPullover = false;

                                    if (targetVehicle.AttachedBlip != null)
                                        targetVehicle.AttachedBlip.Delete();

                                    Screen.LoadingPrompt.Hide();
                                }
                            }
                        }
                        else
                        {
                            Screen.LoadingPrompt.Hide();
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
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnTrafficStopTask -> {ex}");
            }
        }

        // RESET

        static async Task OnCooldownTask()
        {
            IsCooldownActive = true;
            int timer = 60;
            while (timer > 0)
            {
                await Client.Delay(1000);
                timer--;
            }
            IsCooldownActive = false;
            client.DeregisterTickHandler(OnCooldownTask);
        }

        static async void Pullover(Vehicle stoppedVehicle)
        {
            DecorSetBool(stoppedVehicle.Handle, VEHICLE_HAS_BEEN_STOPPED, true);
            IsConductingPullover = true; // Flag that a pullover has started

            if (stoppedVehicle.AttachedBlip != null)
                stoppedVehicle.AttachedBlip.IsFlashing = false;

        }


    }
}

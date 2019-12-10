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

                if (Game.PlayerPed.IsInVehicle() && Client.CurrentVehicle != null)
                {
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

                        long gameTime = GetGameTimer();
                        while ((API.GetGameTimer() - gameTime) < 5000)
                        {
                            await Client.Delay(0);
                        }

                        if (IsCooldownActive)
                        {
                            targetVehicle = null;
                            Wrappers.Helpers.ShowSimpleNotification("~b~Traffic Stops: ~r~Cooldown Active");
                            return;
                        }

                        bool awaitingPullover = true;
                        if (targetVehicle == Client.CurrentVehicle.GetVehicleInFront(DistanceToCheck))
                        {
                            while (awaitingPullover)
                            {
                                API.SetUserRadioControlEnabled(false);

                                await Client.Delay(0);
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
                                }

                                if (Game.IsControlJustPressed(0, Control.Cover))
                                {
                                    targetVehicle.AttachedBlip.IsFlashing = false;
                                    awaitingPullover = false;

                                    if (targetVehicle.AttachedBlip != null)
                                        targetVehicle.AttachedBlip.Delete();

                                    targetVehicle.IsPositionFrozen = false;

                                    DecorSetBool(targetVehicle.Handle, NPC_VEHICLE_IGNORE, true);
                                }
                            }
                        }
                    }

                    //    if (Game.PlayerPed.CurrentVehicle != Client.CurrentVehicle && TargetVehicle == null)
                    //    {
                    //        Screen.ShowNotification($"~b~Traffic Stop: ~w~Must be using a Personal Vehicle from the garage.");
                    //        return;
                    //    }

                    //    // Don't run any of the code if the TargetVehicle is still in control
                    //    if (TargetVehicle != null) return;
                    //    // If we are doing a pull over, don't run any more...
                    //    if (IsConductingPullover) return;

                    //    TargetVehicle = Client.CurrentVehicle.GetVehicleInFront(DistanceToCheck);

                    //    if (TargetVehicle == null) return;
                    //    if (TargetVehicle.Driver.IsPlayer) return;

                    //    // if no driver, don't do anything
                    //    if (TargetVehicle.Driver == null) return;
                    //    // if the driver is dead, don't do anything
                    //    if (TargetVehicle.Driver.IsDead) return;

                    //    bool hasBeenPulledOver = DecorGetBool(TargetVehicle.Handle, VEHICLE_HAS_BEEN_STOPPED);

                    //    if (hasBeenPulledOver)
                    //    {
                    //        Screen.DisplayHelpTextThisFrame($"You have already pulled over this vehicle.");
                    //        return;
                    //    }

                    //    // 5 second timer so we don't try attaching to a bunch of vehicles
                    //    long gameTime = GetGameTimer();
                    //    while ((API.GetGameTimer() - gameTime) < 5000)
                    //    {
                    //        await Client.Delay(0);
                    //    }

                    //    if (IsCooldownActive)
                    //    {
                    //        TargetVehicle = null;
                    //        Wrappers.Helpers.ShowSimpleNotification("~b~Traffic Stops: ~r~Cooldown Active");
                    //        return;
                    //    }

                    //    // If the vehicle matches then we will mark the vehicle and start checking for player inputs
                    //    if (TargetVehicle == Client.CurrentVehicle.GetVehicleInFront(DistanceToCheck))
                    //    {
                    //        AwaitingPullover = true;
                    //        while (AwaitingPullover)
                    //        {
                    //            SetUserRadioControlEnabled(false);

                    //            await Client.Delay(0);
                    //            Screen.DisplayHelpTextThisFrame($"Press ~INPUT_PICKUP~ to initiate a ~b~Traffic Stop");

                    //            if (TargetVehicle.AttachedBlip == null)
                    //            {
                    //                TargetVehicle.AttachBlip();
                    //                TargetVehicle.AttachedBlip.Sprite = BlipSprite.Standard;
                    //                TargetVehicle.AttachedBlip.Color = BlipColor.Red;
                    //            }

                    //            if (Game.IsControlJustPressed(0, Control.Pickup))
                    //            {
                    //                BlipSiren(Client.CurrentVehicle.Handle);
                    //                Pullover(TargetVehicle);
                    //                AwaitingPullover = false;
                    //                return;
                    //            }

                    //            if (TargetVehicle.Position.Distance(Client.CurrentVehicle.Position) > 20f)
                    //            {
                    //                if (TargetVehicle.AttachedBlip != null)
                    //                {
                    //                    if (TargetVehicle.AttachedBlip.Exists())
                    //                    {
                    //                        TargetVehicle.AttachedBlip.Delete();
                    //                    }
                    //                }

                    //                TargetVehicle = null;
                    //                return;
                    //            }
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    if (Game.PlayerPed.IsInVehicle())
                    //    {
                    //        Screen.ShowNotification($"~b~Traffic Stop: ~w~Current vehicle type of ~r~{Game.PlayerPed.CurrentVehicle.ClassType} ~w~is not valid.");
                    //        await Client.Delay(2000);
                    //        Client.TriggerEvent("curiosity:Client:Police:TrafficStops", false);
                    //        Dispose();
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnTrafficStopTask -> {ex}");
            }
        }

        // FOLLOW
        static public void FollowPlayer()
        {
            
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
            
        }


    }
}

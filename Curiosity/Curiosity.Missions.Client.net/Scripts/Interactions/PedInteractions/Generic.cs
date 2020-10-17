﻿using CitizenFX.Core;
using Curiosity.Missions.Client.Classes.PlayerClient;
using Curiosity.Missions.Client.MissionPeds;
using Curiosity.Shared.Client.net.Extensions;
using System.Collections.Generic;
using System.Linq;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.Scripts.Interactions.PedInteractions
{
    class Generic
    {
        static bool IsAnInteractionActive = false;

        static public async void InteractionBreathalyzer(InteractivePed interactivePed)
        {
            if (IsAnInteractionActive) return;

            // check ped is in front of the player
            Ped pedInFront = Game.PlayerPed.GetPedInFront(pedToCheck: interactivePed);

            bool runBreathalyzerChecks = false;

            if (pedInFront == null)
            {
                Wrappers.Helpers.ShowSimpleNotification($"~r~Must be facing the suspect.");
                return;
            }

            if (pedInFront != null)
            {
                if (pedInFront.Exists() && pedInFront.IsAlive)
                {
                    if (pedInFront != interactivePed.Ped) return;

                    // TODO: Add a method for randomly arrested peds also
                    interactivePed.Ped.Task.TurnTo(Game.PlayerPed, 6000);
                    runBreathalyzerChecks = true;
                }
            }

            IsAnInteractionActive = true;

            if (runBreathalyzerChecks)
            {
                Helpers.Animations.AnimationSearch();

                Wrappers.Helpers.ShowSimpleNotification("~w~Performing ~b~Breathalyzer~w~ test...");

                await PluginManager.Delay(3000);
                string bac = $"~g~0.{interactivePed.BloodAlcaholLimit:00}";
                if (interactivePed.BloodAlcaholLimit >= 8)
                {
                    bac = $"~r~0.{interactivePed.BloodAlcaholLimit:00}";
                    interactivePed.IsUnderTheInfluence = true;
                }
                Wrappers.Helpers.ShowSimpleNotification($"~b~BAC ~w~Level: {bac}");
            }
            else
            {
                Wrappers.Helpers.ShowSimpleNotification($"~r~Must be facing the suspect.");
            }
            IsAnInteractionActive = false;
        }

        static public async void InteractionDrugTest(InteractivePed interactivePed)
        {
            if (IsAnInteractionActive) return;

            Ped pedInFront = Game.PlayerPed.GetPedInFront(pedToCheck: interactivePed);

            bool runDrugChecks = false;

            if (pedInFront == null)
            {
                Wrappers.Helpers.ShowSimpleNotification($"~r~Must be facing the suspect.");
                return;
            }

            if (pedInFront != null)
            {
                if (pedInFront.Exists() && pedInFront.IsAlive)
                {
                    if (pedInFront != interactivePed.Ped) return;

                    // TODO: Add a method for randomly arrested peds also
                    interactivePed.Ped.Task.TurnTo(Game.PlayerPed, 6000);
                    runDrugChecks = true;
                }
            }

            IsAnInteractionActive = true;

            if (runDrugChecks)
            {
                string cannabis = interactivePed.IsUsingCannabis ? "~r~" : "~g~";
                string cocaine = interactivePed.IsUsingCocaine ? "~r~" : "~g~";
                Helpers.Animations.AnimationSearch();
                Wrappers.Helpers.ShowSimpleNotification("~w~Performing ~b~Drugalyzer~w~ test...");
                await PluginManager.Delay(3000);
                Wrappers.Helpers.ShowSimpleNotification($"~w~Results:\n~b~  Cannabis~w~: {cannabis}{interactivePed.IsUsingCannabis}\n~b~  Cocaine~w~: {cocaine}{interactivePed.IsUsingCocaine}");
            }
            else
            {
                Wrappers.Helpers.ShowSimpleNotification($"~r~Must be facing the suspect.");
            }
            IsAnInteractionActive = false;
        }

        static public async void InteractionSearch(InteractivePed interactivePed)
        {
            if (IsAnInteractionActive) return;

            // check ped is in front of the player
            Ped pedInFront = Game.PlayerPed.GetPedInFront(pedToCheck: interactivePed);
            Vehicle vehicleInFront = Game.PlayerPed.GetVehicleInFront();

            if (interactivePed.HasBeenSearched) return;
            if (!interactivePed.IsAllowedToBeSearched) return;

            bool CanSearchSuspect = false;

            if (pedInFront == null)
            {
                Wrappers.Helpers.ShowSimpleNotification($"~r~Must be facing the suspect.");
                return;
            }

            if (pedInFront != null)
            {
                if (pedInFront.Exists() && pedInFront.IsAlive)
                {
                    if (pedInFront != interactivePed.Ped) return;
                    // TODO: Add a method for randomly arrested peds also
                    Wrappers.Helpers.ShowSimpleNotification("~b~Searching~w~ the subject...");
                    CanSearchSuspect = true;
                }
            }

            IsAnInteractionActive = true;

            Helpers.Animations.AnimationSearch();

            if (CanSearchSuspect)
            {
                if (interactivePed.IsAllowedToBeSearched)
                {
                    int chanceOfFlee = PluginManager.Random.Next(20);
                    int chanceOfShooting = PluginManager.Random.Next(20);

                    Wrappers.Helpers.ShowSimpleNotification($"~w~Found ~r~{DataClasses.Police.ItemData.illegalItems[PluginManager.Random.Next(DataClasses.Police.ItemData.illegalItems.Count)]}");
                    if (chanceOfFlee >= 13)
                    {
                        List<Vehicle> vehicles = World.GetAllVehicles().Select(m => m).Where(x => x.Position.Distance(interactivePed.Position) < 50f).ToList();

                        vehicles.ForEach(v =>
                        {
                            CitizenFX.Core.Native.API.SetVehicleCanBeUsedByFleeingPeds(v.Handle, false);
                        });

                        PluginManager.TriggerEvent("curiosity:setting:group:leave", interactivePed.Ped.Handle);

                        interactivePed.Ped.Task.ReactAndFlee(Game.PlayerPed);
                        interactivePed.RanFromPolice = true;
                    }
                    else if (chanceOfShooting >= 15)
                    {
                        if (!interactivePed.IsHandcuffed)
                        {
                            Game.PlayerPed.Task.ClearAll();
                            Game.PlayerPed.Task.ClearAllImmediately();

                            PluginManager.TriggerEvent("curiosity:interaction:closeMenu");

                            interactivePed.Ped.DropsWeaponsOnDeath = false;

                            interactivePed.Ped.Weapons.Give(WeaponHash.Pistol, 90, false, true);
                            interactivePed.Ped.Task.ShootAt(Game.PlayerPed);
                            interactivePed.IsWanted = true;
                        }
                    }
                    PluginManager.TriggerEvent("curiosity:interaction:searched", interactivePed.Handle, true);
                }
            }
            IsAnInteractionActive = false;
        }

        public async static void InteractionEnterVehicle(InteractivePed interactivePed)
        {
            if (!DecorExistOn(interactivePed.Ped.Handle, PluginManager.DECOR_NPC_CURRENT_VEHICLE)) return;

            if (interactivePed.Ped.IsInVehicle()) return;

            int vehicleHandle = DecorGetInt(interactivePed.Ped.Handle, PluginManager.DECOR_NPC_CURRENT_VEHICLE);
            Vehicle vehicle = new Vehicle(vehicleHandle);

            if (ClientInformation.IsDeveloper && PluginManager.DeveloperNpcUiEnabled)
            {
                CitizenFX.Core.UI.Screen.ShowNotification($"Vehicle: {vehicle.Handle}");
            }

            interactivePed.Ped.Task.ClearAll();
            interactivePed.Ped.Task.ClearSecondary();

            interactivePed.Ped.LeaveGroup();
            interactivePed.Ped.Task.EnterVehicle(vehicle, VehicleSeat.Driver);

            while (!interactivePed.Ped.IsInVehicle())
            {
                await BaseScript.Delay(0);
            }
            interactivePed.Ped.SetConfigFlag(292, true);
            interactivePed.Ped.CurrentVehicle.Doors.GetAll().ToList<VehicleDoor>().ForEach(d => d.Close());
        }

        public static async void InteractionLeaveVehicle(InteractivePed interactivePed)
        {
            int resistExitChance = PluginManager.Random.Next(30);

            if (!interactivePed.Ped.IsInVehicle()) return;

            DecorSetInt(interactivePed.Ped.Handle, PluginManager.DECOR_NPC_CURRENT_VEHICLE, interactivePed.Ped.CurrentVehicle.Handle);

            if (resistExitChance == 25 || resistExitChance == 29)
            {
                await PluginManager.Delay(500);
                List<string> driverResponse = new List<string>() { "No way!", "Fuck off!", "Not today!", "Shit!", "Uhm.. Nope.", "Get away from me!", "Pig!", "No.", "Never!", "You'll never take me alive, pig!" };
                Wrappers.Helpers.ShowSuspectSubtitle(driverResponse[PluginManager.Random.Next(driverResponse.Count)]);
                await PluginManager.Delay(3000);

                int willRam = PluginManager.Random.Next(5);
                if (willRam == 4)
                    TaskVehicleTempAction(interactivePed.Ped.Handle, interactivePed.Ped.CurrentVehicle.Handle, 28, 3000);

                await PluginManager.Delay(2000);
                TaskVehicleTempAction(interactivePed.Ped.Handle, interactivePed.Ped.CurrentVehicle.Handle, 32, 30000);
                interactivePed.Ped.Task.FleeFrom(Game.PlayerPed);

                PluginManager.TriggerEvent("curiosity:interaction:veh:flee", interactivePed.Handle);
            }
            else
            {
                List<string> driverResponse = new List<string>() { "What's the problem?", "What seems to be the problem, officer?", "Yeah, sure.", "Okay.", "Fine.", "What now?", "Whats up?", "Ummm... O-okay.", "This is ridiculous...", "I'm kind of in a hurry right now.", "Oh what now?!", "No problem.", "Am I being detained?", "Yeah, okay... One moment.", "Okay.", "Uh huh.", "Yep." };
                Wrappers.Helpers.ShowSuspectSubtitle(driverResponse[PluginManager.Random.Next(driverResponse.Count)]);

                interactivePed.Ped.SetConfigFlag(292, false);

                interactivePed.Ped.Task.LeaveVehicle(LeaveVehicleFlags.None);

                await BaseScript.Delay(100);

                PluginManager.TriggerEvent("curiosity:setting:group:join", interactivePed.Ped.Handle);
            }
        }
    }
}

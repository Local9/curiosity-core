using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curiosity.Missions.Client.net.Scripts.Interactions.PedInteractions;
using Curiosity.Shared.Client.net;
using Curiosity.Missions.Client.net.MissionPeds;
using CitizenFX.Core;
using Curiosity.Shared.Client.net.Extensions;

namespace Curiosity.Missions.Client.net.Scripts.Interactions.PedInteractions
{
    class Generic
    {
        static bool IsAnInteractionActive = false;

        static public async void InteractionBreathalyzer(InteractivePed interactivePed)
        {
            if (IsAnInteractionActive) return;

            // check ped is in front of the player
            Ped pedInFront = Game.PlayerPed.GetPedInFront();
            
            bool runBreathalyzerChecks = false;
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

                await Client.Delay(3000);
                string bac = $"~g~0.{interactivePed.BloodAlcaholLimit}";
                if (interactivePed.BloodAlcaholLimit >= 8)
                {
                    bac = $"~r~0.{interactivePed.BloodAlcaholLimit}";
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
            
            Ped pedInFront = Game.PlayerPed.GetPedInFront();
            
            bool runDrugChecks = false;
            
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
                await Client.Delay(3000);
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
            Ped pedInFront = Game.PlayerPed.GetPedInFront();
            Vehicle vehicleInFront = Game.PlayerPed.GetVehicleInFront();

            if (interactivePed.HasBeenSearched) return;

            bool CanSearchSuspect = false;

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
                    Wrappers.Helpers.ShowSimpleNotification($"~w~Found ~r~{DataClasses.Police.ItemData.illegalItems[Client.Random.Next(DataClasses.Police.ItemData.illegalItems.Count)]}");
                    if (Client.Random.Next(10) == 9)
                    {
                        List<Vehicle> vehicles = World.GetAllVehicles().Select(m => m).Where(x => x.Position.Distance(interactivePed.Position) < 50f).ToList();

                        vehicles.ForEach(v =>
                        {
                            CitizenFX.Core.Native.API.SetVehicleCanBeUsedByFleeingPeds(v.Handle, false);
                        });

                        interactivePed.Ped.LeaveGroup();
                        interactivePed.Ped.Task.ReactAndFlee(Game.PlayerPed);
                    }
                    Client.TriggerEvent("curiosity:interaction:searched", interactivePed.NetworkId, true);
                }
            }
            else
            {
                Wrappers.Helpers.ShowSimpleNotification($"~r~Must be facing the suspect.");
            }
            IsAnInteractionActive = false;
        }
    }
}

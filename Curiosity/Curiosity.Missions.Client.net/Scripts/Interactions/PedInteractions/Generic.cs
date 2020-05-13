using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curiosity.Missions.Client.net.Scripts.Interactions.PedInteractions;
using Curiosity.Shared.Client.net;
using Curiosity.Missions.Client.net.MissionPeds;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
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

                        Client.TriggerEvent("curiosity:setting:group:leave", interactivePed.Ped.Handle);

                        interactivePed.Ped.Task.ReactAndFlee(Game.PlayerPed);
                    }
                    else if (Client.Random.Next(10) >= 8) 
                    {
                        interactivePed.Ped.Weapons.Give(WeaponHash.Pistol, 90, false, true);
                        interactivePed.Ped.Task.ShootAt(Game.PlayerPed);
                    }
                    interactivePed.Set(Client.DECOR_INTERACTION_WANTED, true);
                    Client.TriggerEvent("curiosity:interaction:searched", interactivePed.Handle, true);
                }
            }
            else
            {
                Wrappers.Helpers.ShowSimpleNotification($"~r~Must be facing the suspect.");
            }
            IsAnInteractionActive = false;
        }

        public async static void InteractionEnterVehicle(InteractivePed interactivePed)
        {
            if (!DecorExistOn(interactivePed.Ped.Handle, Client.DECOR_NPC_CURRENT_VEHICLE)) return;

            if (interactivePed.Ped.IsInVehicle()) return;

            int vehicleHandle = DecorGetInt(interactivePed.Ped.Handle, Client.DECOR_NPC_CURRENT_VEHICLE);
            Vehicle vehicle = new Vehicle(vehicleHandle);

            if (Classes.PlayerClient.ClientInformation.IsDeveloper() && Client.DeveloperNpcUiEnabled)
            {
                CitizenFX.Core.UI.Screen.ShowNotification($"Vehicle: {vehicle.Handle}");
            }

            interactivePed.Ped.Task.ClearAll();
            interactivePed.Ped.Task.ClearSecondary();

            interactivePed.Ped.LeaveGroup();
            interactivePed.Ped.Task.EnterVehicle(vehicle, VehicleSeat.Driver);

            while(!interactivePed.Ped.IsInVehicle())
            {
                await BaseScript.Delay(0);
            }
            interactivePed.Ped.SetConfigFlag(292, true);
            interactivePed.Ped.CurrentVehicle.Doors.GetAll().ToList<VehicleDoor>().ForEach(d => d.Close());
        }

        public static async void InteractionLeaveVehicle(InteractivePed interactivePed)
        {
            int resistExitChance = Client.Random.Next(30);

            if (!interactivePed.Ped.IsInVehicle()) return;

            DecorSetInt(interactivePed.Ped.Handle, Client.DECOR_NPC_CURRENT_VEHICLE, interactivePed.Ped.CurrentVehicle.Handle);

            if (resistExitChance == 25 || resistExitChance == 29)
            {
                await Client.Delay(500);
                List<string> driverResponse = new List<string>() { "No way!", "Fuck off!", "Not today!", "Shit!", "Uhm.. Nope.", "Get away from me!", "Pig!", "No.", "Never!", "You'll never take me alive, pig!" };
                Wrappers.Helpers.ShowSuspectSubtitle(driverResponse[Client.Random.Next(driverResponse.Count)]);
                await Client.Delay(3000);

                int willRam = Client.Random.Next(5);
                if (willRam == 4)
                    TaskVehicleTempAction(interactivePed.Ped.Handle, interactivePed.Ped.CurrentVehicle.Handle, 28, 3000);

                await Client.Delay(2000);
                TaskVehicleTempAction(interactivePed.Ped.Handle, interactivePed.Ped.CurrentVehicle.Handle, 32, 30000);
                interactivePed.Ped.Task.FleeFrom(Game.PlayerPed);

                Client.TriggerEvent("curiosity:interaction:veh:flee", interactivePed.Handle);
            }
            else
            {
                List<string> driverResponse = new List<string>() { "What's the problem?", "What seems to be the problem, officer?", "Yeah, sure.", "Okay.", "Fine.", "What now?", "Whats up?", "Ummm... O-okay.", "This is ridiculous...", "I'm kind of in a hurry right now.", "Oh what now?!", "No problem.", "Am I being detained?", "Yeah, okay... One moment.", "Okay.", "Uh huh.", "Yep." };
                Wrappers.Helpers.ShowSuspectSubtitle(driverResponse[Client.Random.Next(driverResponse.Count)]);

                interactivePed.Ped.SetConfigFlag(292, false);

                interactivePed.Ped.Task.LeaveVehicle(LeaveVehicleFlags.None);
                
                await BaseScript.Delay(100);

                Client.TriggerEvent("curiosity:setting:group:join", interactivePed.Ped.Handle);
            }
        }
    }
}

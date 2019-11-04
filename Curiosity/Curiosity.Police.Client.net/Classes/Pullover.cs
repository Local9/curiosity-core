﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Classes;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Police.Client.net.Classes
{
    class Pullover
    {
        static Client client = Client.GetInstance();

        static Vehicle playerVehicle;
        static Vehicle vehicleToInteract = null;
        static Ped vehDriver = null;

        static Random random = new Random();

        static protected string PULLED_OVER_DECOR = "curiosity:police:IsPulledOver";
        static protected string WAS_PULLED_OVER_DECOR = "curiosity:police:WasPulledOver";
        static protected string SPEEDING_DECOR = "curiosity:police:Speeding";
        static protected int TIME_BETWEEN_PULLOVERS = 45000;

        static long LastPullover;
        static bool TicketedPed = false;
        static bool IsDriverFleeing = false;
        static bool IsDriverOnFoot = false;

        static RelationshipGroup FleeingDriverRelationship;

        static public void Init()
        {
            FleeingDriverRelationship = World.AddRelationshipGroup("FLEE");

            client.RegisterTickHandler(OnControlAction);
            client.RegisterTickHandler(OnDrawMarker);

            client.RegisterEventHandler("onClientResourceStop", new Action<string>(OnClientResourceStop));

            client.RegisterEventHandler("curiosity:Client:Police:ReleaseAI", new Action(OnReleaseAi));
            client.RegisterEventHandler("curiosity:Client:Police:ReleaseTicketedAI", new Action(OnReleaseAiTicket));
            client.RegisterEventHandler("curiosity:Client:Police:ReleaseSpeedingTicketedAI", new Action(OnReleaseAiSpeedingTicket));
            client.RegisterEventHandler("curiosity:Client:Police:Speeding", new Action<int, bool>(OnVehicleSpeeding));

            API.DecorRegister(PULLED_OVER_DECOR, 3);
            API.DecorRegister(WAS_PULLED_OVER_DECOR, 3);
            API.DecorRegister(SPEEDING_DECOR, 3);
        }

        static void OnClientResourceStop(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;
            AwardPlayer(false);
        }

        static void OnVehicleSpeeding(int vehicleHandle, bool locked)
        {
            if (!locked) return;

            Vehicle vehicle = new Vehicle(vehicleHandle);

            if (vehicle.Driver.IsPlayer) return;

            API.DecorSetBool(vehicle.Handle, SPEEDING_DECOR, true);
            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Speeding Violation", $"Plate: {vehicle.Mods.LicensePlate}", $"~b~Model: ~s~{vehicle.LocalizedName}~n~~b~1st Color:~s~~n~ {vehicle.Mods.PrimaryColor}~n~~b~2nd Color:~s~~n~ {vehicle.Mods.SecondaryColor}", 2);
        }

        static void OnReleaseAiSpeedingTicket()
        {
            if (TicketedPed) return;

            TicketedPed = true;

            if (vehicleToInteract.Driver != null)
            {
                if (vehicleToInteract.Driver.IsAlive)
                {
                    Client.TriggerServerEvent("curiosity:Server:Bank:IncreaseCash", Player.PlayerInformation.playerInfo.Wallet, random.Next(16, 26));
                    Client.TriggerServerEvent("curiosity:Server:Skills:Increase", $"{Enums.Skills.policexp}", random.Next(3, 6));
                    Client.TriggerServerEvent("curiosity:Server:Skills:Increase", $"knowledge", random.Next(3, 6));
                }
            }

            IssuedTicket();
        }

        static void OnReleaseAiTicket()
        {
            if (TicketedPed) return;

            TicketedPed = true;

            if (vehicleToInteract.Driver != null)
            {
                if (vehicleToInteract.Driver.IsAlive)
                {
                    Client.TriggerServerEvent("curiosity:Server:Bank:IncreaseCash", Player.PlayerInformation.playerInfo.Wallet, random.Next(5, 16));
                    Client.TriggerServerEvent("curiosity:Server:Skills:Increase", $"{Enums.Skills.policexp}", random.Next(1, 3));
                    Client.TriggerServerEvent("curiosity:Server:Skills:Increase", $"knowledge", random.Next(1, 3));
                }
            }

            IssuedTicket();
        }

        static async void IssuedTicket()
        {
            CommonFunctions.PlayScenario("CODE_HUMAN_MEDIC_TIME_OF_DEATH");

            API.DecorSetBool(vehicleToInteract.Handle, WAS_PULLED_OVER_DECOR, true);

            await Client.Delay(10000);

            CommonFunctions.PlayScenario("forcestop");

            OnReleaseAi();
        }

        static void OnReleaseAi()
        {
            if (vehicleToInteract != null)
            {
                if (vehicleToInteract.IsStopped)
                {
                    ReleasePed();
                }
            }
        }

        static async Task OnControlAction()
        {
            if (
                ControlHelper.IsControlPressed(CitizenFX.Core.Control.VehicleHorn, false)
                && Game.PlayerPed.IsInVehicle()
                && Game.PlayerPed.IsInPoliceVehicle)
            {

                if (Player.PlayerInformation.playerInfo.Skills["policexp"].Value < 2500)
                {

                    Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Experience Requirement", $"~b~Remaining: ~s~{2500 - Player.PlayerInformation.playerInfo.Skills["policexp"].Value:#,##0}", "Sorry you require 2,500 experience to do a pull over.", 2);
                    return;
                }

                if (Player.PlayerInformation.playerInfo.Skills["knowledge"].Value < 1000)
                {

                    Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Experience Requirement", $"~b~Remaining: ~s~{1000 - Player.PlayerInformation.playerInfo.Skills["knowledge"].Value:#,##0}", "Sorry you require 1,000 knowledge to do a pull over.", 2);
                    return;
                }

                if (!Environment.Job.DutyManager.IsPoliceJobActive)
                {
                    Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Police Dept", $"", "Must be a police officer to pull over a pedestrian.", 2);
                    return;
                }

                if (Environment.Job.DutyManager.IsOnCallout)
                {
                    Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Police Dept", $"", "You are currently on an active callout.", 2);
                    return;
                }

                if ((Game.GameTime - LastPullover) <= TIME_BETWEEN_PULLOVERS && TicketedPed)
                {
                    float timeToWait = (TIME_BETWEEN_PULLOVERS - (Game.GameTime - LastPullover)) / 1000;

                    string color = "~r~";

                    if (timeToWait <= 25) color = "~y~";

                    if (timeToWait <= 5) color = "~g~";

                    Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Police Dept", $"Time Left: {color}{timeToWait}s", $"You can only do pull overs every ~b~{TIME_BETWEEN_PULLOVERS / 1000}~s~ seconds.", 2);
                    await Client.Delay(1000);
                    return;
                }

                if (!Game.PlayerPed.CurrentVehicle.IsSirenActive) return;

                if (Game.PlayerPed.CurrentVehicle.Driver != Game.PlayerPed) return;

                if (vehicleToInteract != null)
                {
                    if (vehicleToInteract.Exists())
                    {
                        Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Police Dept", $"", "You still have an active pull over.", 2);
                        return;
                    }
                    vehicleToInteract = null;
                }

                playerVehicle = Game.PlayerPed.CurrentVehicle;

                Vector3 basePos = playerVehicle.Position;
                RaycastResult raycast = World.RaycastCapsule(basePos, playerVehicle.GetOffsetPosition(new Vector3(0f, 7f, 0f)) + new Vector3(0f, 0f, -0.4f), 1f, (IntersectOptions)71, playerVehicle);

                if (raycast.DitHitEntity && raycast.HitEntity.Model.IsVehicle)
                {
                    vehicleToInteract = (CitizenFX.Core.Vehicle)raycast.HitEntity;
                }

                if (vehicleToInteract.Driver.IsPlayer) return;

                if (vehicleToInteract != null)
                {
                    if (vehicleToInteract.Driver != null)
                    {
                        if (!vehicleToInteract.Driver.Exists())
                        {
                            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Police Dept", $"", "You cannot pull over empty vehicles.", 2);
                            vehicleToInteract = null;
                            return;
                        }

                        if (API.DecorGetBool(vehicleToInteract.Handle, WAS_PULLED_OVER_DECOR))
                        {
                            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Police Dept", $"", "Has already been pulled over, you cannot harass pedestrians.", 2);
                            vehicleToInteract = null;
                            return;
                        }

                        LastPullover = Game.GameTime;

                        Function.Call(Hash.BLIP_SIREN, Game.PlayerPed.CurrentVehicle.Handle);

                        if (API.NetworkHasControlOfEntity(vehicleToInteract.Handle))
                        {
                            if (random.Next(5) == 1)
                            {
                                int driver = vehicleToInteract.Driver.Handle;
                                int vehicle = vehicleToInteract.Handle;

                                if (vehicleToInteract.Driver.IsAlive)
                                {
                                    vehDriver = vehicleToInteract.Driver;

                                    vehDriver.RelationshipGroup = FleeingDriverRelationship;

                                    FleeingDriverRelationship.SetRelationshipBetweenGroups(Client.PlayerRelationshipGroup, Relationship.Dislike);

                                    vehDriver.Task.FightAgainstHatedTargets(50f);

                                    API.SetEntityAsMissionEntity(driver, true, true);
                                    API.SetEntityAsMissionEntity(vehicle, true, true);

                                    API.SetVehicleCanBeUsedByFleeingPeds(vehicle, true);
                                    API.SetBlockingOfNonTemporaryEvents(driver, true);
                                    API.SetPedFleeAttributes(driver, 2, true);
                                    API.TaskReactAndFleePed(driver, Game.PlayerPed.Handle);
                                    IsDriverFleeing = true;
                                    IsDriverOnFoot = false;

                                    TicketedPed = false;

                                    if (!vehicleToInteract.IsStopped)
                                        Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "~r~WARNING", $"", "They're getting away!", 2);

                                    await Client.Delay(1000);

                                    client.RegisterTickHandler(TickDriverFleeing);
                                }
                            }
                            else
                            {
                                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Notification", $"Plate: {vehicleToInteract.Mods.LicensePlate}", "Vehicle in front is stopping.", 2);
                                TicketedPed = false;
                                API.SetVehicleHalt(vehicleToInteract.Handle, 40f, 1, false);
                                API.DecorSetBool(vehicleToInteract.Handle, PULLED_OVER_DECOR, true);
                            }
                        }
                    }
                }

                await Client.Delay(500);
            }

            await Task.FromResult(0);
        }

        static async Task OnDrawMarker()
        {
            while (true)
            {
                await Client.Delay(0);
                if (vehicleToInteract != null && !IsDriverFleeing)
                {
                    World.DrawMarker(MarkerType.UpsideDownCone, vehicleToInteract.Position + new Vector3(0f, 0f, 1.6f), Vector3.Zero, Vector3.Zero, new Vector3(0.3f, 0.3f, 0.3f), System.Drawing.Color.FromArgb(255, 255, 255, 255), true);

                    if (vehicleToInteract.IsStopped)
                    {
                        vehicleToInteract.IsPositionFrozen = true;
                    }

                    if (vehicleToInteract.IsStopped && Game.PlayerPed.Position.DistanceToSquared(vehicleToInteract.Position) > 200f)
                    {
                        ReleasePed();
                    }
                }

                if (IsDriverFleeing)
                { 
                    float distance = NativeWrappers.GetDistanceBetween(Game.PlayerPed.Position, vehDriver.Position);
                    if (distance > 250f)
                    {
                        Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Police Dept", $"", "They got away.", 2);
                        AwardPlayer(false);
                        IsDriverFleeing = false;
                        client.DeregisterTickHandler(TickDriverFleeing);
                    }
                }
            }
        }

        static async Task TickDriverFleeing()
        {
            while (IsDriverFleeing)
            {
                await Client.Delay(100);

                if (vehicleToInteract == null)
                {
                    break;
                }

                if (vehicleToInteract.IsStopped)
                {
                    if (vehicleToInteract.Driver.IsInVehicle())
                    {
                        vehDriver.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);

                        vehDriver.DropsWeaponsOnDeath = false;

                        int driver = vehDriver.Handle;

                        if (random.Next(10) == 1)
                        {
                            API.GiveWeaponToPed(driver, (uint)WeaponHash.PumpShotgun, 100, true, false);
                        }

                        if (random.Next(20) == 1)
                        {
                            API.GiveWeaponToPed(driver, (uint)WeaponHash.CarbineRifle, 100, true, false);
                        }
                    }
                    else
                    {
                        if (!IsDriverOnFoot)
                        {
                            FleeingDriverRelationship.SetRelationshipBetweenGroups(Client.PlayerRelationshipGroup, Relationship.Hate);
                            vehDriver.Task.FightAgainst(Game.PlayerPed, -1);
                            IsDriverOnFoot = true;
                        }
                    }
                }

                if (Game.PlayerPed.IsDead)
                {
                    AwardPlayer(false);
                    break;
                }

                if (vehDriver.IsDead)
                {
                    AwardPlayer();
                    break;
                }
            }
            client.DeregisterTickHandler(TickDriverFleeing);
        }

        static void ReleasePed()
        {
            API.DecorSetBool(vehicleToInteract.Handle, PULLED_OVER_DECOR, false);
            vehicleToInteract.IsPositionFrozen = false;
            vehicleToInteract = null;
        }

        static void AwardPlayer(bool giveAward = true)
        {
            if (giveAward)
            {
                if (IsDriverFleeing && !IsDriverOnFoot)
                {
                    Client.TriggerServerEvent("curiosity:Server:Bank:IncreaseCash", Player.PlayerInformation.playerInfo.Wallet, random.Next(50, 71));
                    Client.TriggerServerEvent("curiosity:Server:Skills:Increase", $"{Enums.Skills.policexp}", random.Next(10, 30));
                    Client.TriggerServerEvent("curiosity:Server:Skills:Increase", $"knowledge", random.Next(3, 8));
                }
                else if (IsDriverFleeing && IsDriverOnFoot)
                {
                    Client.TriggerServerEvent("curiosity:Server:Bank:IncreaseCash", Player.PlayerInformation.playerInfo.Wallet, random.Next(90, 111));
                    Client.TriggerServerEvent("curiosity:Server:Skills:Increase", $"{Enums.Skills.policexp}", random.Next(30, 51));
                    Client.TriggerServerEvent("curiosity:Server:Skills:Increase", $"knowledge", random.Next(5, 11));
                }
                else
                {
                    Client.TriggerServerEvent("curiosity:Server:Bank:IncreaseCash", Player.PlayerInformation.playerInfo.Wallet, random.Next(20, 31));
                    Client.TriggerServerEvent("curiosity:Server:Skills:Increase", $"{Enums.Skills.policexp}", random.Next(4, 10));
                    Client.TriggerServerEvent("curiosity:Server:Skills:Increase", $"knowledge", random.Next(1, 3));
                }
            }

            IsDriverFleeing = false;
            IsDriverOnFoot = false;

            TicketedPed = giveAward;

            if (vehDriver != null)
            {
                vehDriver.MarkAsNoLongerNeeded();
                vehDriver = null;
            }

            if (vehicleToInteract != null)
            {
                vehicleToInteract.MarkAsNoLongerNeeded();
                vehicleToInteract = null;
            }
        }
    }
}

using System;
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

        static Vehicle vehicle;
        static Vehicle vehFound = null;
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
            API.DecorSetBool(vehicle.Handle, SPEEDING_DECOR, true);
            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Speeding Violation", $"Plate: {vehicle.Mods.LicensePlate}", $"~b~Model: ~s~{vehicle.LocalizedName}~n~~b~1st Color:~s~~n~ {vehicle.Mods.PrimaryColor}~n~~b~2nd Color:~s~~n~ {vehicle.Mods.SecondaryColor}", 2);
        }

        static void OnReleaseAiSpeedingTicket()
        {
            if (vehFound.Driver != null)
            {
                if (vehDriver.IsAlive)
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
            if (vehFound.Driver != null)
            {
                if (vehDriver.IsAlive)
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

            TicketedPed = true;
            API.DecorSetBool(vehFound.Handle, WAS_PULLED_OVER_DECOR, true);

            await Client.Delay(10000);

            CommonFunctions.PlayScenario("forcestop");

            OnReleaseAi();
        }

        static void OnReleaseAi()
        {
            if (vehFound != null)
            {
                if (vehFound.IsStopped)
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

                if (Player.PlayerInformation.playerInfo.Skills["policexp"].Value < 4500)
                {

                    Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Experience Requirement", $"~b~Remaining: ~s~{4500 - Player.PlayerInformation.playerInfo.Skills["policexp"].Value:#,##0}", "Sorry you require 4,500 experience to do a pull over.", 2);
                    return;
                }

                if (Player.PlayerInformation.playerInfo.Skills["knowledge"].Value < 1000)
                {

                    Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Experience Requirement", $"~b~Remaining: ~s~{1000 - Player.PlayerInformation.playerInfo.Skills["knowledge"].Value:#,##0}", "Sorry you require 1,000 knowledge to do a pull over.", 2);
                    return;
                }

                if (!Environment.Job.DutyManager.IsOnDuty)
                {
                    Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Police Dept", $"", "Must be on active duty to pull over a pedestrian.", 2);
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

                if (vehFound != null)
                {
                    Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Police Dept", $"", "You still have an active pull over.", 2);
                    return;
                }

                vehicle = Game.PlayerPed.CurrentVehicle;

                Vector3 basePos = vehicle.Position;
                RaycastResult raycast = World.RaycastCapsule(basePos, vehicle.GetOffsetPosition(new Vector3(0f, 7f, 0f)) + new Vector3(0f, 0f, -0.4f), 1f, (IntersectOptions)71, vehicle);

                if (raycast.DitHitEntity && raycast.HitEntity.Model.IsVehicle)
                {
                    vehFound = (CitizenFX.Core.Vehicle)raycast.HitEntity;
                }

                if (vehFound != null)
                {
                    if (vehFound.Driver != null)
                    {
                        if (!vehFound.Driver.Exists())
                        {
                            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Police Dept", $"", "You cannot pull over empty vehicles.", 2);
                            vehFound = null;
                            return;
                        }

                        if (API.DecorGetBool(vehFound.Handle, WAS_PULLED_OVER_DECOR))
                        {
                            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Police Dept", $"", "Has already been pulled over, you cannot harass pedestrians.", 2);
                            vehFound = null;
                            return;
                        }

                        LastPullover = Game.GameTime;

                        Function.Call(Hash.BLIP_SIREN, Game.PlayerPed.CurrentVehicle.Handle);

                        if (API.NetworkHasControlOfEntity(vehFound.Handle))
                        {
                            if (random.Next(5) == 1)
                            {
                                int driver = vehFound.Driver.Handle;
                                int vehicle = vehFound.Handle;

                                if (vehFound.Driver.IsAlive)
                                {
                                    vehDriver = vehFound.Driver;

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

                                    if (!vehFound.IsStopped)
                                        Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "~r~WARNING", $"", "They're getting away!", 2);

                                    await Client.Delay(1000);

                                    client.RegisterTickHandler(TickDriverFleeing);
                                }
                            }
                            else
                            {
                                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Notification", $"Plate: {vehFound.Mods.LicensePlate}", "Vehicle in front is stopping.", 2);
                                TicketedPed = false;
                                API.SetVehicleHalt(vehFound.Handle, 40f, 1, false);
                                API.DecorSetBool(vehFound.Handle, PULLED_OVER_DECOR, true);
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
                if (vehFound != null && !IsDriverFleeing)
                {
                    World.DrawMarker(MarkerType.UpsideDownCone, vehFound.Position + new Vector3(0f, 0f, 1.6f), Vector3.Zero, Vector3.Zero, new Vector3(0.3f, 0.3f, 0.3f), System.Drawing.Color.FromArgb(255, 255, 255, 255), true);

                    if (vehFound.IsStopped)
                    {
                        vehFound.IsPositionFrozen = true;
                    }

                    if (vehFound.IsStopped && Game.PlayerPed.Position.DistanceToSquared(vehFound.Position) > 200f)
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

                if (vehFound == null)
                {
                    break;
                }

                if (vehFound.IsStopped)
                {
                    if (vehFound.Driver.IsInVehicle())
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
            API.DecorSetBool(vehFound.Handle, PULLED_OVER_DECOR, false);
            vehFound.IsPositionFrozen = false;
            vehFound = null;
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

            if (vehFound != null)
            {
                vehFound.MarkAsNoLongerNeeded();
                vehFound = null;
            }
        }
    }
}

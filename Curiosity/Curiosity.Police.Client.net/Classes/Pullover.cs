using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Shared.Client.net.Enums;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Police.Client.net.Classes
{
    class Pullover
    {
        static Client client = Client.GetInstance();

        static Vehicle vehicle;
        static Vehicle vehFound = null;

        static Random random = new Random();

        static protected string PULLED_OVER_DECOR = "curiosity:police:IsPulledOver";
        static protected string WAS_PULLED_OVER_DECOR = "curiosity:police:WasPulledOver";
        static protected string SPEEDING_DECOR = "curiosity:police:Speeding";
        static protected int TIME_BETWEEN_PULLOVERS = 45000;

        static long LastPullover;
        static bool TicketedPed = false;

        static public void Init()
        {
            client.RegisterTickHandler(OnControlAction);
            client.RegisterTickHandler(OnDrawMarker);

            client.RegisterEventHandler("curiosity:Client:Police:ReleaseAI", new Action(OnReleaseAi));
            client.RegisterEventHandler("curiosity:Client:Police:ReleaseTicketedAI", new Action(OnReleaseAiTicket));
            client.RegisterEventHandler("curiosity:Client:Police:ReleaseSpeedingTicketedAI", new Action(OnReleaseAiSpeedingTicket));
            client.RegisterEventHandler("curiosity:Client:Police:Speeding", new Action<int, bool>(OnVehicleSpeeding));

            API.DecorRegister(PULLED_OVER_DECOR, 3);
            API.DecorRegister(WAS_PULLED_OVER_DECOR, 3);
            API.DecorRegister(SPEEDING_DECOR, 3);
        }

        static void OnVehicleSpeeding(int vehicleHandle, bool locked)
        {
            if (!locked) return;

            Vehicle vehicle = new Vehicle(vehicleHandle);
            API.DecorSetBool(vehicle.Handle, SPEEDING_DECOR, true);
            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Speeding Violation", $"Plate: {vehicle.Mods.LicensePlate}", $"~b~Primary Color:~s~~n~ {vehicle.Mods.PrimaryColor}~n~~b~Secondary Color:~s~~n~ {vehicle.Mods.SecondaryColor}", 2);
        }

        static void OnReleaseAiSpeedingTicket()
        {
            Client.TriggerServerEvent("curiosity:Server:Bank:IncreaseCash", Player.PlayerInformation.playerInfo.Wallet, random.Next(20, 31));
            Client.TriggerServerEvent("curiosity:Server:Skills:Increase", $"{Enums.Skills.policexp}", random.Next(2, 6));
            Client.TriggerServerEvent("curiosity:Server:Skills:Increase", $"knowledge", random.Next(2, 6));

            OnReleaseTicketedAi();
        }

        static void OnReleaseAiTicket()
        {
            Client.TriggerServerEvent("curiosity:Server:Bank:IncreaseCash", Player.PlayerInformation.playerInfo.Wallet, random.Next(10, 21));
            Client.TriggerServerEvent("curiosity:Server:Skills:Increase", $"{Enums.Skills.policexp}", random.Next(2, 6));
            Client.TriggerServerEvent("curiosity:Server:Skills:Increase", $"knowledge", random.Next(2, 6));

            OnReleaseTicketedAi();
        }

        static void OnReleaseTicketedAi()
        {
            TicketedPed = true;
            API.DecorSetBool(vehFound.Handle, WAS_PULLED_OVER_DECOR, true);

            if (vehFound != null)
            {
                if (vehFound.IsStopped)
                {
                    ReleasePed();
                }
            }
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
                RaycastResult raycast = World.Raycast(basePos, vehicle.GetOffsetPosition(new Vector3(0f, 7f, 0f)) + new Vector3(0f, 0f, -0.4f), (IntersectOptions)71, vehicle);

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

                        Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Notification", $"Plate: {vehFound.Mods.LicensePlate}", "Vehicle in front is stopping.", 2);

                        Ped ped = vehFound.Driver;

                        Function.Call(Hash.BLIP_SIREN, Game.PlayerPed.CurrentVehicle.Handle);

                        if (API.NetworkHasControlOfEntity(vehFound.Handle))
                        {
                            TicketedPed = false;
                            API.SetVehicleHalt(vehFound.Handle, 40f, 1, false);
                            API.DecorSetBool(vehFound.Handle, PULLED_OVER_DECOR, true);
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
                if (vehFound != null)
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
            }
        }

        static void ReleasePed()
        {
            API.DecorSetBool(vehFound.Handle, PULLED_OVER_DECOR, false);
            vehFound.IsPositionFrozen = false;
            vehFound = null;
        }
    }
}

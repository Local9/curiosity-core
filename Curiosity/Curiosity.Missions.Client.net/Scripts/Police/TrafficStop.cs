using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using Curiosity.Shared.Client.net.Helpers;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Missions.Client.net.DataClasses;
using Curiosity.Shared.Client.net.Enums;

namespace Curiosity.Missions.Client.net.Scripts.Police
{
    class TrafficStop
    {
        static Client client = Client.GetInstance();

        static float DistanceToCheck = 10.0f;

        static Vehicle TargetVehicle;
        static bool AwaitingPullover = true;
        static bool IsConductingPullover = false;

        // states
        static bool IsVehicleFleeing = false;
        static bool IsVehicleStopped = false;
        // states for menu
        static bool CanSearchVehicle = false;
        static bool IsDriverUnderTheInfluence = false;
        static bool HasDriverBeenQuestioned = false;

        // Vehicle Data
        static string VehicleRegisterationYear;
        // Register Driver Data
        static string VehicleRegisterationDriversName, VehicleRegisterationDriversDateOfBirth;
        // Stolen Vehicle Driver Data
        static string StolenDriverName, StolenDriverDateOfBirth;

        // Notification Messages
        static string NotificationFlagMessage = "~g~NONE";
        static string NotificationFlagCannabis = "~g~Negative";
        static string NotificationFlagCocaine = "~g~Negative";

        public static void Setup()
        {
            if (Classes.PlayerClient.ClientInformation.IsDeveloper())
            {
                client.RegisterTickHandler(OnTrafficStopTask);
                client.RegisterTickHandler(OnTrafficStopStateTask);
                Screen.ShowNotification("~b~Traffic Stops~s~: ~g~Enabled");
            }
        }

        public static void Dispose()
        {
            if (Classes.PlayerClient.ClientInformation.IsDeveloper())
            {
                client.DeregisterTickHandler(OnTrafficStopTask);
                client.DeregisterTickHandler(OnTrafficStopStateTask);
                Screen.ShowNotification("~b~Traffic Stops~s~: ~r~Disabled");
            }
        }

        static async Task OnTrafficStopTask()
        {
            try
            {
                await BaseScript.Delay(0);

                if (Game.PlayerPed.IsInVehicle())
                {
                    if (Game.PlayerPed.CurrentVehicle.ClassType == VehicleClass.Emergency)
                    {
                        // Don't run any of the code if the TargetVehicle is still in control
                        if (TargetVehicle != null) return;
                        // If we are doing a pull over, don't run any more...
                        if (IsConductingPullover) return;

                        TargetVehicle = Client.CurrentVehicle.GetVehicleInDirection(DistanceToCheck);

                        if (TargetVehicle == null) return;

                        // if no driver, don't do anything
                        if (TargetVehicle.Driver == null) return;
                        // if the driver is dead, don't do anything
                        if (TargetVehicle.Driver.IsDead) return;

                        // 5 second timer so we don't try attaching to a bunch of vehicles
                        long gameTime = GetGameTimer();
                        while ((API.GetGameTimer() - gameTime) < 5000)
                        {
                            await Client.Delay(0);
                        }
                        // If the vehicle matches then we will mark the vehicle and start checking for player inputs
                        if (TargetVehicle == Client.CurrentVehicle.GetVehicleInDirection(DistanceToCheck))
                        {
                            TargetVehicle.AttachBlip();
                            TargetVehicle.AttachedBlip.Sprite = BlipSprite.Standard;
                            TargetVehicle.AttachedBlip.Color = BlipColor.Red;

                            while (AwaitingPullover)
                            {
                                await Client.Delay(0);

                                if (Game.PlayerPed.IsInVehicle())
                                    Screen.DisplayHelpTextThisFrame($"Press ~INPUT_PICKUP~ to start a ~b~traffic Stop");

                                if (Game.IsControlJustReleased(0, Control.Pickup))
                                {
                                    BlipSiren(Client.CurrentVehicle.Handle);
                                    Pullover(TargetVehicle);
                                    AwaitingPullover = false;
                                }

                                if (TargetVehicle.Position.Distance(Client.CurrentVehicle.Position) > 200f)
                                {
                                    if (TargetVehicle.AttachedBlip != null)
                                    {
                                        if (TargetVehicle.AttachedBlip.Exists())
                                        {
                                            TargetVehicle.AttachedBlip.Delete();
                                        }
                                    }

                                    TargetVehicle = null;
                                    AwaitingPullover = false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnTrafficStopTask -> {ex}");
            }
        }

        // If the IsVehicleStopped flag is set to true, then stop the vehicle
        static async Task OnTrafficStopStateTask()
        {
            try
            {
                await Client.Delay(0);
                if (TargetVehicle == null)
                {
                    await Client.Delay(500);
                    return;
                } // Check every 500ms, don't spam the client

                if (IsVehicleStopped && TargetVehicle.IsEngineRunning)
                {
                    SetVehicleEngineOn(TargetVehicle.Handle, false, false, true);

                    if (GetEntitySpeed(TargetVehicle.Handle) <= 1f)
                    {
                        RollDownWindows(TargetVehicle.Handle);
                    }
                }

                if (TargetVehicle.Position.Distance(Game.PlayerPed.Position) >= 300f)
                {
                    ShowNotification("Dispatch", "They got away", string.Empty);
                    Reset();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnTrafficStopStateTask -> {ex}");
            }
        }

        static void Reset()
        {
            if (TargetVehicle != null)
            {
                if (TargetVehicle.AttachedBlip != null)
                {
                    if (TargetVehicle.AttachedBlip.Exists())
                    {
                        TargetVehicle.AttachedBlip.Delete();
                    }
                }
            }

            TargetVehicle.Driver.IsPersistent = false;
            TargetVehicle.Driver.MarkAsNoLongerNeeded();
            TargetVehicle.IsPersistent = false;
            TargetVehicle.MarkAsNoLongerNeeded();

            IsVehicleStopped = false;
            IsVehicleFleeing = false;

            AwaitingPullover = true;
            IsConductingPullover = false;

            TargetVehicle = null;

            NotificationFlagMessage = "~g~NONE";
            NotificationFlagCannabis = "~g~Negative";
            NotificationFlagCocaine = "~g~Negative";

            HasDriverBeenQuestioned = false;
            IsDriverUnderTheInfluence = false;
            CanSearchVehicle = false;
        }

        static async void Pullover(Vehicle stoppedVehicle)
        {
            IsConductingPullover = true; // Flag that a pullover has started

            Ped stoppedDriver = stoppedVehicle.Driver;
            // make sure the driver has full health
            stoppedDriver.Health = 200;
            // Make sure they are set not to be removed
            stoppedDriver.IsPersistent = true;
            stoppedVehicle.IsPersistent = true;
            // Get current location
            string zoneKey = GetNameOfZone(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z);
            string currentZoneName = Zones.Locations[zoneKey];
            // Set flee chances
            int DriverChanceOfFlee, DriverChanceOfShootOrFlee;
            if (currentZoneName == "Davis" || currentZoneName == "Rancho" || currentZoneName == "Strawberry")
            {
                DriverChanceOfFlee = Client.Random.Next(23, 30);
                DriverChanceOfShootOrFlee = Client.Random.Next(1, 5);
            }
            else if (stoppedVehicle.Model.IsBike)
            {
                DriverChanceOfFlee = Client.Random.Next(23, 30);
                DriverChanceOfShootOrFlee = 0;
            }
            else
            {
                DriverChanceOfFlee = Client.Random.Next(30);
                DriverChanceOfShootOrFlee = Client.Random.Next(5);
            }
            // Ped Data
            // generate a name
            string firstname;
            if (stoppedDriver.Gender == Gender.Female)
            {
                firstname = PedNames.FirstNameMale[Client.Random.Next(PedNames.FirstNameMale.Count - 1)];
            }
            else
            {
                firstname = PedNames.FirstNameFemale[Client.Random.Next(PedNames.FirstNameFemale.Count - 1)];
            }

            string surname = PedNames.Surname[Client.Random.Next(PedNames.Surname.Count - 1)];
            // Generate a date of birth
            DateTime StartDateForDriverDoB = new DateTime(1949, 1, 1);
            int Range = (DateTime.Today - StartDateForDriverDoB).Days;
            StartDateForDriverDoB.AddDays(Client.Random.Next(Range));

            VehicleRegisterationDriversName = $"{firstname} {surname}";
            VehicleRegisterationDriversDateOfBirth = StartDateForDriverDoB.ToString("yyyy-mm-dd");
            VehicleRegisterationYear = $"{Client.Random.Next(1990, DateTime.Now.Year)}";

            // Notification Flags
            int InsuredRand = Client.Random.Next(9);
            int RegisteredRand = Client.Random.Next(13);
            int StolenRand = Client.Random.Next(25);

            bool IsVehicleStolen = false;

            if (StolenRand == 24)
            {
                NotificationFlagMessage = "~r~STOLEN";
                IsVehicleStolen = true;
            } 
            else if (RegisteredRand == 12)
            {
                NotificationFlagMessage = "~r~UNREGISTERED";
                VehicleRegisterationYear = "~r~UNREGISTERED";
            }
            else if (InsuredRand == 8)
            {
                NotificationFlagMessage = "~r~UNINSURED";
            }
            // Lost ID Chance
            int DriverLostIdChance = Client.Random.Next(100);
            int DriverDifferentName = Client.Random.Next(100);

            // Update ped data if vehicle is stolen or the driver is not the registered driver
            if (IsVehicleStolen || DriverDifferentName >= 95)
            {
                string stolenFirstname;
                if (stoppedDriver.Gender == Gender.Female)
                {
                    stolenFirstname = PedNames.FirstNameMale[Client.Random.Next(PedNames.FirstNameMale.Count - 1)];
                }
                else
                {
                    stolenFirstname = PedNames.FirstNameFemale[Client.Random.Next(PedNames.FirstNameFemale.Count - 1)];
                }

                string stolenSurname = PedNames.Surname[Client.Random.Next(PedNames.Surname.Count - 1)];
                // Generate a date of birth
                DateTime StolenStartDateForDriverDoB = new DateTime(1949, 1, 1);
                int StolenRange = (DateTime.Today - StolenStartDateForDriverDoB).Days;
                StolenStartDateForDriverDoB.AddDays(Client.Random.Next(StolenRange));

                StolenDriverName = $"{stolenFirstname} {stolenSurname}";
                StolenDriverDateOfBirth = StolenStartDateForDriverDoB.ToString("yyyy-mm-dd");

                DriverChanceOfFlee = Client.Random.Next(25, 30);
                DriverChanceOfShootOrFlee = Client.Random.Next(1, 5);
                DriverLostIdChance = Client.Random.Next(80, 100);
            }

            // Driver Blood Alcahol Limit
            int DriverBreathalyzer = Client.Random.Next(100);
            int DriverBloodAlcaholLimit = 0;
            IsDriverUnderTheInfluence = false;

            if (DriverBreathalyzer > 60)
            {
                DriverBloodAlcaholLimit = Client.Random.Next(1, 7);
                if (DriverBreathalyzer > 88)
                {
                    DriverBloodAlcaholLimit = Client.Random.Next(8, 10);
                    IsDriverUnderTheInfluence = true;
                    DriverChanceOfFlee = Client.Random.Next(25, 30);
                    if (DriverBreathalyzer > 95)
                    {
                        DriverChanceOfShootOrFlee = Client.Random.Next(1, 5);
                        DriverBloodAlcaholLimit = Client.Random.Next(10, 20);
                    }
                }
            }

            // Driver Cannabis
            int DriverCannabisChance = Client.Random.Next(100);
            int DriverCocaineChance = Client.Random.Next(100);

            if (DriverCannabisChance > 85)
            {
                NotificationFlagCannabis = "~r~Positive";
                IsDriverUnderTheInfluence = true;
                DriverChanceOfFlee = Client.Random.Next(18, 30);
            }

            if (DriverCocaineChance > 90)
            {
                NotificationFlagCannabis = "~r~Positive";
                IsDriverUnderTheInfluence = true;
                DriverChanceOfFlee = Client.Random.Next(18, 30);
            }

            // Search Vehicle
            CanSearchVehicle = false;
            int SearchChance = Client.Random.Next(100);
            if (SearchChance >= 90)
            {
                CanSearchVehicle = true;
                DriverLostIdChance = Client.Random.Next(70, 100);
            }

            HasDriverBeenQuestioned = false;

            // Drunk Flags
            if (IsDriverUnderTheInfluence)
            {
                if (!HasAnimSetLoaded("MOVE_M@DRUNK@VERYDRUNK"))
                {
                    RequestAnimSet("MOVE_M@DRUNK@VERYDRUNK");
                    while (!HasAnimSetLoaded("MOVE_M@DRUNK@VERYDRUNK"))
                    {
                        await Client.Delay(0);
                    }
                }
            }
            // Break window
            if (IsVehicleStolen)
            {
                int BrokenWindow = Client.Random.Next(3);
                if (BrokenWindow == 2)
                {
                    SmashVehicleWindow(stoppedVehicle.Handle, Client.Random.Next(3));
                }
            }
            // AI Control
            int TimeAfterStop = Client.Random.Next(5, 30) * 1000;
            int TimeAfterShoot = Client.Random.Next(5, 30) * 1000;

            if (DriverChanceOfFlee == 29)
            {
                TrafficStopVehicleFlee(stoppedVehicle, stoppedDriver);
                ALPR(stoppedVehicle);
            }
            else if (DriverChanceOfFlee == 28)
            {
                if (DriverChanceOfShootOrFlee == 4)
                {
                    TrafficStopVehicleStop(stoppedVehicle);
                    await Client.Delay(TimeAfterStop);
                    TrafficStopVehicleShoot(stoppedDriver);
                    await Client.Delay(TimeAfterShoot);
                    TrafficStopVehicleFlee(stoppedVehicle, stoppedDriver);
                }
                else
                {
                    bool IsDriverGoingToFlee = true;
                    TrafficStopVehicleStop(stoppedVehicle);
                    // Wait for the player to get close, then run
                    while (IsDriverGoingToFlee)
                    {
                        await Client.Delay(0);
                        float DistanceToVehicle = stoppedDriver.Position.Distance(Game.PlayerPed.Position);
                        if (DistanceToVehicle <= 5 && !Game.PlayerPed.IsInVehicle())
                        {
                            TrafficStopVehicleFlee(stoppedVehicle, stoppedDriver);
                            IsDriverGoingToFlee = false;
                        }
                    }
                }
            }
            else
            {
                TrafficStopVehicleStop(stoppedVehicle);
            }
        }

        static void TrafficStopVehicleStop(Vehicle vehicle)
        {
            IsVehicleStopped = true;
            ALPR(vehicle);
        }

        static async void TrafficStopVehicleShoot(Ped ped)
        {
            IsVehicleStopped = false;
            ped.IsPersistent = true;
            ped.Weapons.Give(WeaponHash.CombatPistol, 30, false, true);
            ped.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
            await Client.Delay(1000);
            ped.Task.ShootAt(Game.PlayerPed, 10000000, FiringPattern.FullAuto);
        }

        static async void TrafficStopVehicleFlee(Vehicle vehicle, Ped ped)
        {
            try
            {
                IsVehicleStopped = false;
                IsVehicleFleeing = true;
                SetVehicleEngineOn(vehicle.Handle, true, false, true);
                SetVehicleCanBeUsedByFleeingPeds(vehicle.Handle, true);

                if (vehicle.Driver == null)
                {
                    ped.Task.EnterVehicle(vehicle, VehicleSeat.Driver, 20000, 5f);
                }

                await Client.Delay(1000);

                int willRam = Client.Random.Next(5);

                if (willRam == 4)
                {
                    TaskVehicleTempAction(vehicle.Driver.Handle, vehicle.Handle, 28, 3000);
                }

                await Client.Delay(0);

                TaskVehicleTempAction(vehicle.Driver.Handle, vehicle.Handle, 32, 30000);
                vehicle.Driver.Task.FleeFrom(Game.PlayerPed);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TrafficStopVehicleFlee -> {ex}");
            }
        }

        static async void ALPR(Vehicle vehicle)
        {
            VehicleHash vehicleHash = (VehicleHash)vehicle.Model.Hash;
            string NumberPlate = vehicle.Mods.LicensePlate;

            ShowNotification("Dispatch", "Getting Information", string.Empty);
            await Client.Delay(0);
            ShowNotification("Dispatch", "LSPD Database", $"Plate: ~y~{NumberPlate}~s~\nModel: ~y~{GetLabelText(GetDisplayNameFromVehicleModel((uint)vehicleHash))}~s~\nVehicle Class: {vehicle.ClassType}");
        }

        static void ShowNotification(string title, string subtitle, string message)
        {
            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, title, subtitle, message, 2);
        }
    }
}

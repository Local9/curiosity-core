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

        static Client client = Client.GetInstance();
        static bool IsScenarioPlaying = false;

        static float DistanceToCheck = 15.0f;

        static public Vehicle TargetVehicle;
        static public Ped StoppedDriver;
        static bool AwaitingPullover = true;
        static bool IsConductingPullover = false;

        // states
        static bool IsVehicleFleeing = false;
        static bool IsVehicleStopped = false;
        static bool IsCooldownActive = false;
        // states for menu
        static public bool IsDriverUnderTheInfluence = false;
        static public bool IsDriverUnderTheInfluenceOfDrugs = false;
        static public bool HasVehicleBeenStolen = false;
        static public bool CanDriverBeArrested = false;
        static public bool IsCarryingIllegalItems = false;
        static public bool HasSearchedSuspect = false;
        static bool IsInteractionActive = false;
        static public bool VehicleDriverReverseWithPlayer = true;

        static bool CanSearchVehicle = false;
        static bool HasDriverBeenAskedForID = false;
        static bool IsVehicleDriverMimicking = false;
        static bool IsVehicleDriverFollowing = false;
        static bool IsDriverFollowing = false;
        static bool HasRanDriverID = false;

        // Ped Data
        static int DriverBloodAlcaholLimit;
        static int DriverLostIdChance;
        static int DriverAttitude;

        static int DriverCannabisChance;
        static int DriverCocaineChance;

        static bool IsDriverGoingToFlee = false;
        static int IsDriverGoingToFleeChance;

        // Vehicle Data
        static string VehicleRegisterationYear;
        // Register Driver Data
        static string VehicleRegisterationDriversName, VehicleRegisterationDriversDateOfBirth;
        // Stolen Vehicle Driver Data
        static string StolenDriverName, StolenDriverDateOfBirth;
        // Active Identification
        static string IdentityName, IdentityDateOfBirth;

        // Notification Messages
        static string NotificationFlagVehicle = "~g~NONE";
        static string NotificationFlagOffense = "~g~NONE";
        static string NotificationFlagCitations;
        static string NotificationFlagCannabis = "~g~Negative";
        static string NotificationFlagCocaine = "~g~Negative";

        public static void Setup()
        {
            int policeXp = Classes.PlayerClient.ClientInformation.playerInfo.Skills["policexp"].Value;
            int knowledge = Classes.PlayerClient.ClientInformation.playerInfo.Skills["knowledge"].Value;
            if (policeXp >= 2500 && knowledge >= 1000)
            {
                client.RegisterTickHandler(OnTrafficStopTask);
                client.RegisterTickHandler(OnTrafficStopStateTask);
                client.RegisterTickHandler(OnEmoteCheck);
                Screen.ShowNotification("~b~Traffic Stops~s~: ~g~Enabled");
            }
            else
            {
                Screen.ShowNotification($"~b~Traffic Stops~s~: ~o~Missing Req\n~b~Remaining:\n  ~b~PoliceXp: ~w~{2500 - policeXp}\n  ~b~Knowledge: ~w~{1000 - knowledge}");
            }
        }

        public static void Dispose()
        {
            Reset();
            client.DeregisterTickHandler(OnTrafficStopTask);
            client.DeregisterTickHandler(OnTrafficStopStateTask);
            client.DeregisterTickHandler(OnEmoteCheck);
            // Screen.ShowNotification("~b~Traffic Stops~s~: ~r~Disabled");
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

                        if (Game.PlayerPed.CurrentVehicle != Client.CurrentVehicle)
                        {
                            Screen.ShowNotification($"~b~Traffic Stop: ~w~Must be using a personal vehicle from the garage.");
                            return;
                        }

                        TargetVehicle = Client.CurrentVehicle.GetVehicleInFront(DistanceToCheck);
                        
                        if (TargetVehicle == null) return;
                        if (TargetVehicle.Driver.IsPlayer) return;

                        // if no driver, don't do anything
                        if (TargetVehicle.Driver == null) return;
                        // if the driver is dead, don't do anything
                        if (TargetVehicle.Driver.IsDead) return;

                        bool hasBeenPulledOver = DecorGetBool(TargetVehicle.Handle, VEHICLE_HAS_BEEN_STOPPED);

                        if (hasBeenPulledOver)
                        {
                            Screen.DisplayHelpTextThisFrame($"You have already pulled over this vehicle.");
                            return;
                        }

                        // 5 second timer so we don't try attaching to a bunch of vehicles
                        long gameTime = GetGameTimer();
                        while ((API.GetGameTimer() - gameTime) < 5000)
                        {
                            await Client.Delay(0);
                        }

                        if (IsCooldownActive)
                        {
                            TargetVehicle = null;
                            Wrappers.Helpers.ShowSimpleNotification("~b~Traffic Stops: ~r~Cooldown Active");
                            return;
                        }

                        // If the vehicle matches then we will mark the vehicle and start checking for player inputs
                        if (TargetVehicle == Client.CurrentVehicle.GetVehicleInFront(DistanceToCheck))
                        {
                            AwaitingPullover = true;
                            while (AwaitingPullover)
                            {
                                SetUserRadioControlEnabled(false);

                                await Client.Delay(0);
                                Screen.DisplayHelpTextThisFrame($"Press ~INPUT_PICKUP~ to initiate a ~b~Traffic Stop");

                                if (TargetVehicle.AttachedBlip == null)
                                {
                                    TargetVehicle.AttachBlip();
                                    TargetVehicle.AttachedBlip.Sprite = BlipSprite.Standard;
                                    TargetVehicle.AttachedBlip.Color = BlipColor.Red;
                                }

                                if (Game.IsControlJustPressed(0, Control.Pickup))
                                {
                                    BlipSiren(Client.CurrentVehicle.Handle);
                                    Pullover(TargetVehicle);
                                    AwaitingPullover = false;
                                    return;
                                }

                                if (TargetVehicle.Position.Distance(Client.CurrentVehicle.Position) > 20f)
                                {
                                    if (TargetVehicle.AttachedBlip != null)
                                    {
                                        if (TargetVehicle.AttachedBlip.Exists())
                                        {
                                            TargetVehicle.AttachedBlip.Delete();
                                        }
                                    }

                                    TargetVehicle = null;
                                    Reset();
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Game.PlayerPed.IsInVehicle())
                        {
                            Screen.ShowNotification($"~b~Traffic Stop: ~w~Current vehicle type of ~r~{Game.PlayerPed.CurrentVehicle.ClassType} ~w~is not valid.");
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

                if (TargetVehicle.Driver == null)
                {
                    await Client.Delay(500);
                    return;
                }

                if (TargetVehicle != null)
                    API.NetworkRequestControlOfEntity(TargetVehicle.NetworkId);

                if (StoppedDriver != null)
                    API.NetworkRequestControlOfEntity(StoppedDriver.NetworkId);

                if (IsVehicleStopped && TargetVehicle.IsEngineRunning && !IsVehicleDriverMimicking)
                {
                    TargetVehicle.IsEngineRunning = false;

                    if (TargetVehicle.Speed <= 1f)
                    {
                        TargetVehicle.Windows.RollDownAllWindows();
                    }
                }

                // Mimick
                if (IsVehicleStopped && Game.PlayerPed.IsInVehicle() && !IsVehicleDriverMimicking && TargetVehicle.Driver != null && !ArrestPed.IsPedCuffed)
                {
                    Screen.DisplayHelpTextThisFrame("Press ~INPUT_COVER~ to move the ~b~Vehicle");

                    if (Game.IsControlJustPressed(0, Control.Cover))
                    {
                        EnableMimicking();
                        await Client.Delay(100);
                    }
                }

                if (!IsVehicleStopped && Game.PlayerPed.IsInVehicle() && IsVehicleDriverMimicking)
                {
                    Screen.DisplayHelpTextThisFrame("Press ~INPUT_COVER~ to ~b~stop moving the Vehicle");
                    
                    if (Game.IsControlJustPressed(0, Control.Cover))
                    {
                        DisableMimick();
                        await Client.Delay(100);
                    }
                }

                if (TargetVehicle.Position.Distance(Game.PlayerPed.Position) > 30f && TargetVehicle.Position.Distance(Game.PlayerPed.Position) < 40f && IsVehicleStopped)
                {
                    Wrappers.Helpers.ShowNotification("Dispatch", "~r~Please stay on scene...", string.Empty);
                }

                if (TargetVehicle.Position.Distance(Game.PlayerPed.Position) >= 300f)
                {
                    if (TargetVehicle.AttachedBlip != null)
                        Wrappers.Helpers.ShowNotification("Dispatch", "~r~Traffic Stop Reset", string.Empty);

                    Reset();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnTrafficStopStateTask -> {ex}");
            }
        }

        // MIMICKING
        static async void EnableMimicking()
        {
            IsVehicleDriverMimicking = true;
            IsVehicleStopped = false;

            string animationDict = "misscarsteal3pullover";

            RequestAnimDict(animationDict);
            while (!HasAnimDictLoaded(animationDict))
            {
                await Client.Delay(0);
            }

            Screen.ShowNotification($"The ~r~{GetLabelText(GetDisplayNameFromVehicleModel((uint)TargetVehicle.Model.Hash))} ~w~is now mimicking you.");
            Game.PlayerPed.Task.PlayAnimation(animationDict, "pull_over_left", 8.0f, -1, (AnimationFlags)49);
            await Client.Delay(1100);
            Game.PlayerPed.Task.ClearSecondary();
            SetPedIntoVehicle(StoppedDriver.Handle, TargetVehicle.Handle, 0);
            await Client.Delay(10);

            Vector3 mimicStartPosition = Game.PlayerPed.Position;

            while (IsVehicleDriverMimicking)
            {
                if (mimicStartPosition.Distance(Game.PlayerPed.Position) > 30f)
                {
                    DisableMimick();
                }

                if ((Game.PlayerPed.CurrentVehicle.Speed * 2.237) > 15f)
                {
                    DisableMimick();
                }

                await Client.Delay(0);
                Vector3 speedVect = GetEntitySpeedVector(Game.PlayerPed.CurrentVehicle.Handle, true);

                if (speedVect.Y > 0f && VehicleDriverReverseWithPlayer)
                {
                    SetVehicleForwardSpeed(TargetVehicle.Handle, Game.PlayerPed.CurrentVehicle.Speed);
                }
                else if (speedVect.Y < 0f && VehicleDriverReverseWithPlayer)
                {
                    SetVehicleForwardSpeed(TargetVehicle.Handle, -1 * Game.PlayerPed.CurrentVehicle.Speed);
                }
                TargetVehicle.SteeringAngle = Game.PlayerPed.CurrentVehicle.SteeringAngle;

                if (!StoppedDriver.IsInVehicle() && !TargetVehicle.IsDriveable)
                {
                    DisableMimick();
                }

                while (TargetVehicle.IsInAir)
                {
                    await Client.Delay(0);
                }
            }
        }

        static public async void DisableMimick()
        {
            IsVehicleDriverMimicking = false;
            await Client.Delay(100);
            Screen.ShowNotification($"The ~r~{GetLabelText(GetDisplayNameFromVehicleModel((uint)TargetVehicle.Model.Hash))} ~w~is no longer mimicking you.");
            IsVehicleStopped = true;
            await Client.Delay(2000);
            SetPedIntoVehicle(StoppedDriver.Handle, TargetVehicle.Handle, -1);
        }

        // FOLLOW
        static public void FollowPlayer()
        {
            
        }

        // GetOutOfCar
        static public async void LeaveVehicle()
        {
            if (IsVehicleStopped)
            {
                if (Client.speechType == SpeechType.NORMAL)
                {
                    Wrappers.Helpers.ShowOfficerSubtitle("Can you step out of the car for me, please?");
                }
                else
                {
                    Wrappers.Helpers.ShowOfficerSubtitle("Get the fuck out of the car.");
                }
                int resistExitChance = Client.Random.Next(30);
                if (HasVehicleBeenStolen)
                {
                    resistExitChance = Client.Random.Next(27, 30);
                }
                IsDriverGoingToFleeChance = Client.Random.Next(10);
                if (resistExitChance == 25 || resistExitChance == 29 || IsDriverGoingToFlee)
                {
                    IsVehicleStopped = false;
                    IsVehicleDriverMimicking = false;
                    TargetVehicle.IsEngineRunning = true;
                    await Client.Delay(500);
                    List<string> driverResponse = new List<string>() { "No way!", "Fuck off!", "Not today!", "Shit!", "Uhm.. Nope.", "Get away from me!", "Pig!", "No.", "Never!", "You'll never take me alive, pig!" };
                    Wrappers.Helpers.ShowSuspectSubtitle(driverResponse[Client.Random.Next(driverResponse.Count)]);
                    await Client.Delay(3000);
                    TrafficStopVehicleFlee(TargetVehicle, StoppedDriver);
                }
                else
                {
                    List<string> driverResponse = new List<string>() { "What's the problem?", "What seems to be the problem, officer?", "Yeah, sure.", "Okay.", "Fine.", "What now?", "Whats up?", "Ummm... O-okay.", "This is ridiculous...", "I'm kind of in a hurry right now.", "Oh what now?!", "No problem.", "Am I being detained?", "Yeah, okay... One moment.", "Okay.", "Uh huh.", "Yep." };
                    Wrappers.Helpers.ShowSuspectSubtitle(driverResponse[Client.Random.Next(driverResponse.Count)]);
                    StoppedDriver.Task.LeaveVehicle(LeaveVehicleFlags.None);
                    StoppedDriver.PedGroup.Add(Game.PlayerPed, true);
                }
            }
        }

        static public void ReturnToVehicle()
        {
            if (IsVehicleStopped)
            {
                Wrappers.Helpers.ShowOfficerSubtitle("Get back in the car, please.");
                StoppedDriver.LeaveGroup();
                StoppedDriver.Task.EnterVehicle(TargetVehicle, VehicleSeat.Driver, -1, 2f, 0);
            }
        }

        // RESET
        static public void Reset(bool issueExperience = false)
        {
            SetUserRadioControlEnabled(true);

            if (TargetVehicle != null)
            {
                if (TargetVehicle.AttachedBlip != null)
                {
                    if (TargetVehicle.AttachedBlip.Exists())
                    {
                        TargetVehicle.AttachedBlip.Delete();
                    }
                }
                TargetVehicle.IsPersistent = false;
            }

            if (StoppedDriver != null)
            {
                API.SetEnableHandcuffs(StoppedDriver.Handle, false);
                StoppedDriver.SetConfigFlag(292, false);
                StoppedDriver.LeaveGroup();
                StoppedDriver.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                StoppedDriver.IsPersistent = false;
                API.TaskSetBlockingOfNonTemporaryEvents(StoppedDriver.Handle, false);
            }

            IsVehicleStopped = false;
            IsVehicleFleeing = false;
            HasSearchedSuspect = false;

            IsConductingPullover = false;

            TargetVehicle = null;

            NotificationFlagVehicle = "~g~NONE";
            NotificationFlagCannabis = "~g~Negative";
            NotificationFlagCocaine = "~g~Negative";

            HasDriverBeenAskedForID = false;
            IsDriverUnderTheInfluence = false;
            IsDriverUnderTheInfluenceOfDrugs = false;
            IsCarryingIllegalItems = false;
            CanDriverBeArrested = false;

            CanSearchVehicle = false;
            HasVehicleBeenStolen = false;

            ArrestPed.IsPedBeingArrested = false;
            ArrestPed.IsPedCuffed = false;
            ArrestPed.ArrestedPed = null;
            ArrestPed.IsPedGrabbed = false;
            ArrestPed.PedInHandcuffs = null;

            Extras.VehicleTow.Reset();
            Extras.Coroner.Reset();

            // Extras.PoliceArrestVehicle.HasPedBeenPickedUp = false;

            MenuHandler.SuspectMenu.LogMessage("Closing menu, Reset has been called");
            client.DeregisterTickHandler(MenuHandler.SuspectMenu.OnMenuTask);

            if (issueExperience)
            {
                Client.TriggerServerEvent("curiosity:Server:Missions:TrafficStop", string.Empty);
                client.RegisterTickHandler(OnCooldownTask);
            }
        }

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

        static public async void ResetPed(bool delete = false)
        {
            if (StoppedDriver != null)
            {
                StoppedDriver.IsPersistent = false;
                API.TaskSetBlockingOfNonTemporaryEvents(StoppedDriver.Handle, false);

                if (delete)
                {
                    StoppedDriver.Fade(false);
                    await Client.Delay(1000);
                    StoppedDriver.Delete();
                }
            }
        }

        static public void ResetVehicle()
        {
            SetUserRadioControlEnabled(true);

            if (TargetVehicle != null)
            {
                if (TargetVehicle.AttachedBlip != null)
                {
                    if (TargetVehicle.AttachedBlip.Exists())
                    {
                        TargetVehicle.AttachedBlip.Delete();
                    }
                }
                TargetVehicle.IsPersistent = false;
            }

            IsVehicleStopped = false;
            IsVehicleFleeing = false;

            IsConductingPullover = false;

            TargetVehicle = null;

            NotificationFlagVehicle = "~g~NONE";
            NotificationFlagCannabis = "~g~Negative";
            NotificationFlagCocaine = "~g~Negative";

            CanSearchVehicle = false;
            HasVehicleBeenStolen = false;
        }

        static async void Pullover(Vehicle stoppedVehicle)
        {
            DecorSetBool(TargetVehicle.Handle, VEHICLE_HAS_BEEN_STOPPED, true);

            IsConductingPullover = true; // Flag that a pullover has started
            HasRanDriverID = false;

            StoppedDriver = new Ped(stoppedVehicle.Driver.Handle); // Make a new reference of the driver
            if (!DecorGetBool(StoppedDriver.Handle, IS_TRAFFIC_STOPPED_PED))
            {
                DecorSetBool(StoppedDriver.Handle, IS_TRAFFIC_STOPPED_PED, true);
            }
            // make sure the driver has full health
            StoppedDriver.Health = 200;
            // Make sure they are set not to be removed
            StoppedDriver.IsPersistent = true;
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
            if (StoppedDriver.Gender == Gender.Female)
            {
                firstname = PedNames.FirstNameMale[Client.Random.Next(PedNames.FirstNameMale.Count)];
            }
            else
            {
                firstname = PedNames.FirstNameMale[Client.Random.Next(PedNames.FirstNameMale.Count)];
            }

            string surname = PedNames.Surname[Client.Random.Next(PedNames.Surname.Count)];
            // Generate a date of birth
            DateTime StartDateForDriverDoB = new DateTime(1949, 1, 1);
            double Range = (DateTime.Today - StartDateForDriverDoB).TotalDays;
            VehicleRegisterationDriversDateOfBirth = StartDateForDriverDoB.AddDays(Client.Random.Next((int)Range)).ToString("yyyy-MM-dd");

            VehicleRegisterationDriversName = $"{firstname} {surname}";
            VehicleRegisterationYear = $"{Client.Random.Next(1990, DateTime.Now.Year)}";

            // Notification Flags
            int InsuredRand = Client.Random.Next(9);
            int RegisteredRand = Client.Random.Next(13);
            int StolenRand = Client.Random.Next(25);

            HasVehicleBeenStolen = false;

            if (StolenRand == 24)
            {
                NotificationFlagVehicle = "~r~STOLEN";
                HasVehicleBeenStolen = true;

                CanDriverBeArrested = true;
            } 
            else if (RegisteredRand == 12)
            {
                NotificationFlagVehicle = "~r~UNREGISTERED";
                VehicleRegisterationYear = "~r~UNREGISTERED";
            }
            else if (InsuredRand == 8)
            {
                NotificationFlagVehicle = "~r~UNINSURED";
            }
            // Attitude
            DriverAttitude = Client.Random.Next(100);
            // Lost ID Chance
            DriverLostIdChance = Client.Random.Next(100);
            int DriverDifferentName = Client.Random.Next(100);

            // Update ped data if vehicle is stolen or the driver is not the registered driver
            if (HasVehicleBeenStolen || DriverDifferentName >= 95)
            {
                CanDriverBeArrested = true;
                string stolenFirstname;
                if (StoppedDriver.Gender == Gender.Female)
                {
                    stolenFirstname = PedNames.FirstNameMale[Client.Random.Next(PedNames.FirstNameMale.Count)];
                }
                else
                {
                    stolenFirstname = PedNames.FirstNameFemale[Client.Random.Next(PedNames.FirstNameFemale.Count)];
                }

                string stolenSurname = PedNames.Surname[Client.Random.Next(PedNames.Surname.Count)];
                // Generate a date of birth
                DateTime StolenStartDateForDriverDoB = new DateTime(1949, 1, 1);
                double StolenRange = (DateTime.Today - StolenStartDateForDriverDoB).TotalDays;
                StolenDriverDateOfBirth = StolenStartDateForDriverDoB.AddDays(Client.Random.Next((int)StolenRange)).ToString("yyyy-MM-dd");

                StolenDriverName = $"{stolenFirstname} {stolenSurname}";

                DriverChanceOfFlee = Client.Random.Next(25, 30);
                DriverChanceOfShootOrFlee = Client.Random.Next(1, 5);
                DriverLostIdChance = Client.Random.Next(80, 100);
            }

            // Driver Blood Alcahol Limit
            int DriverBreathalyzer = Client.Random.Next(100);
            DriverBloodAlcaholLimit = 0;
            IsDriverUnderTheInfluence = false;

            if (DriverBreathalyzer > 60)
            {
                DriverBloodAlcaholLimit = Client.Random.Next(1, 7);
                if (DriverBreathalyzer > 88)
                {
                    CanDriverBeArrested = true;
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

            // Driver Under the Influence
            DriverCannabisChance = Client.Random.Next(100);
            DriverCocaineChance = Client.Random.Next(100);

            if (DriverCannabisChance > 85)
            {
                CanDriverBeArrested = true;
                IsDriverUnderTheInfluenceOfDrugs = true;
                NotificationFlagCannabis = "~r~Positive";
                DriverChanceOfFlee = Client.Random.Next(18, 30);
            }

            if (DriverCocaineChance > 90)
            {
                CanDriverBeArrested = true;
                IsDriverUnderTheInfluenceOfDrugs = true;
                NotificationFlagCannabis = "~r~Positive";
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

            HasDriverBeenAskedForID = false;

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
            if (HasVehicleBeenStolen)
            {
                CanDriverBeArrested = true;
                int BrokenWindow = Client.Random.Next(3);
                if (BrokenWindow == 2)
                {
                    SmashVehicleWindow(stoppedVehicle.Handle, Client.Random.Next(3));
                }
                SetVehicleIsStolen(stoppedVehicle.Handle, true);
            }
            // AI Control
            int TimeAfterStop = Client.Random.Next(5, 30) * 1000;
            int TimeAfterShoot = Client.Random.Next(5, 30) * 1000;

            if (DriverChanceOfFlee == 29)
            {
                TrafficStopVehicleFlee(stoppedVehicle, StoppedDriver);
                ALPR(stoppedVehicle);
            }
            else if (DriverChanceOfFlee == 28)
            {
                if (DriverChanceOfShootOrFlee == 4)
                {
                    TrafficStopVehicleStop(stoppedVehicle);
                    await Client.Delay(TimeAfterStop);
                    TrafficStopVehicleShoot(StoppedDriver);
                    await Client.Delay(TimeAfterShoot);
                    TrafficStopVehicleFlee(stoppedVehicle, StoppedDriver);
                }
                else
                {
                    IsDriverGoingToFlee = true;
                    TrafficStopVehicleStop(stoppedVehicle);
                    // Wait for the player to get close, then run
                    while (IsDriverGoingToFlee)
                    {
                        await Client.Delay(0);
                        float DistanceToVehicle = StoppedDriver.Position.Distance(Game.PlayerPed.Position);
                        if (DistanceToVehicle <= 5 && !Game.PlayerPed.IsInVehicle())
                        {
                            TrafficStopVehicleFlee(stoppedVehicle, StoppedDriver);
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

            client.RegisterTickHandler(MenuHandler.SuspectMenu.OnMenuTask);
            API.TaskSetBlockingOfNonTemporaryEvents(StoppedDriver.Handle, true);
            
            ALPR(vehicle);
        }

        static async void TrafficStopVehicleShoot(Ped ped)
        {
            IsVehicleStopped = false;
            ped.IsPersistent = true;
            StoppedDriver.SetConfigFlag(292, false);
            ped.Weapons.Give(WeaponHash.CombatPistol, 30, false, true);
            ped.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
            await Client.Delay(1000);
            ped.Task.ShootAt(Game.PlayerPed, 10000000, FiringPattern.FullAuto);
        }

        static public async void TrafficStopVehicleFlee(Vehicle vehicle, Ped ped)
        {
            try
            {
                API.TaskSetBlockingOfNonTemporaryEvents(StoppedDriver.Handle, false);
                StoppedDriver.SetConfigFlag(292, false);
                
                IsVehicleStopped = false;
                IsVehicleFleeing = true;
                vehicle.IsEngineRunning = true;
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

        internal static async void InteractionRelease()
        {
            if (Client.speechType == SpeechType.NORMAL)
            {
                Wrappers.Helpers.ShowOfficerSubtitle("Alright, you're free to go.");
            }
            else
            {
                Wrappers.Helpers.ShowOfficerSubtitle("Get out of here before I change my mind.");
            }
            List<string> DriverResponse = new List<string>() { "Okay, thanks.", "Thanks.", "Thank you officer, have a nice day!", "Thanks, bye!", "I'm free to go? Okay, bye!" };
            if (DriverAttitude >= 50 && DriverAttitude < 80)
            {
                DriverResponse = new List<string>() { "Alright.", "Okay.", "Good.", "Okay, bye.", "Okay, goodbye officer.", "Later.", "Bye bye.", "Until next time." };
            }
            else if (DriverAttitude >= 80 && DriverAttitude < 95)
            {
                DriverResponse = new List<string>() { "Bye, asshole...", "Ugh.. Finally.", "Damn cops...", "Until next time.", "Its about time, pig" };
            }
            await Client.Delay(2000);
            Wrappers.Helpers.ShowSuspectSubtitle(DriverResponse[Client.Random.Next(DriverResponse.Count)]);
            Reset(true);
        }

        internal static async void InteractionIssueWarning()
        {
            List<string> OfficerResponse;
            List<string> DriverResponse = new List<string>() { "Thanks.", "Thank you officer.", "Okay, thank you.", "Okay, thank you officer.", "Thank you so much!", "Alright, thanks!", "Yay! Thank you!", "I'll be more careful next time!", "Sorry about that!" }; ;

            if (Client.speechType == SpeechType.NORMAL)
            {
                OfficerResponse = new List<string>() { "You can go, but don't do it again.", "Don't make me pull you over again!", "Have a good day. Be a little more careful next time.", "I'll let you off with a warning this time." };
            }
            else
            {
                OfficerResponse = new List<string>() { "Don't do that again.", "Don't make me pull you over again!", "I'll let you go this time.", "I'll let you off with a warning this time." };
            }

            if (DriverAttitude >= 50 && DriverAttitude < 80)
            {
                DriverResponse = new List<string>() { "Thanks... I guess...", "Yeah, whatever.", "Finally.", "Ugh..", };
            }
            else if (DriverAttitude >= 80 && DriverAttitude < 95)
            {
                DriverResponse = new List<string>() { "Uh huh, bye.", "Yeah, whatever.", "Finally.", "Ugh..", "Prick." };
            }
            else if (DriverAttitude >= 95)
            {
                DriverResponse = new List<string>() { "Troublesum said fuck you too buddy!", "Yea, well don't kill yourself trying" };
            }

            Wrappers.Helpers.ShowOfficerSubtitle(OfficerResponse[Client.Random.Next(OfficerResponse.Count)]);
            await Client.Delay(2000);
            Wrappers.Helpers.ShowSuspectSubtitle(DriverResponse[Client.Random.Next(DriverResponse.Count)]);
            await Client.Delay(2000);
            Reset(true);
        }

        static public async void InteractionRequestPedIdentification()
        {
            if (StoppedDriver == null)
            {
                Wrappers.Helpers.ShowNotification("No No No", "Traffic Stop Required", string.Empty, NotificationCharacter.CHAR_LESTER);
                return;
            }

            if (Game.PlayerPed.Position.Distance(StoppedDriver.Position) <= 5)
            {
                string officerSubtitle = string.Empty;
                if (Client.speechType == SpeechType.NORMAL)
                {
                    officerSubtitle = DataClasses.Police.LinesOfSpeech.OfficerNormalQuotes[Client.Random.Next(DataClasses.Police.LinesOfSpeech.OfficerNormalQuotes.Count)];
                }
                else
                {
                    officerSubtitle = DataClasses.Police.LinesOfSpeech.OfficerAggresiveQuotes[Client.Random.Next(DataClasses.Police.LinesOfSpeech.OfficerAggresiveQuotes.Count)];
                }
                Screen.ShowSubtitle($"~o~Officer: ~w~{officerSubtitle}");
                
                await Client.Delay(2000);

                if (DriverLostIdChance > 95)
                {
                    Screen.ShowSubtitle($"~b~Driver: ~w~Sorry officer, I don't have it on me...");
                }
                else
                {
                    string driverResponse = string.Empty;
                    if (DriverAttitude < 50)
                    {
                        driverResponse = DataClasses.Police.LinesOfSpeech.DriverResponseNormalIdentity[Client.Random.Next(DataClasses.Police.LinesOfSpeech.DriverResponseNormalIdentity.Count)];
                    }
                    else if (DriverAttitude >= 50 && DriverAttitude < 80)
                    {
                        driverResponse = DataClasses.Police.LinesOfSpeech.DriverResponseRushedIdentity[Client.Random.Next(DataClasses.Police.LinesOfSpeech.DriverResponseRushedIdentity.Count)];
                    }
                    else if (DriverAttitude >= 80)
                    {
                        driverResponse = DataClasses.Police.LinesOfSpeech.DriverResponseAngeredIdentity[Client.Random.Next(DataClasses.Police.LinesOfSpeech.DriverResponseAngeredIdentity.Count)];
                    }
                    Screen.ShowSubtitle($"~b~Driver: ~w~{driverResponse}");

                    // Set the notification to show stolen information
                    IdentityName = VehicleRegisterationDriversName;
                    IdentityDateOfBirth = VehicleRegisterationDriversDateOfBirth;

                    if (HasVehicleBeenStolen)
                    {
                        IdentityName = StolenDriverName;
                        IdentityDateOfBirth = StolenDriverDateOfBirth;
                    }

                    Wrappers.Helpers.ShowNotification("Driver's ID", string.Empty, $"~w~Name: ~y~{IdentityName}\n~w~DOB: ~y~{IdentityDateOfBirth}");
                    
                    HasDriverBeenAskedForID = true;
                }
            }
            else
            {
                Screen.ShowNotification("~r~You need to be closer to the driver!");
            }
        }

        static public async void InteractionRunPedIdentification()
        {
            if (!HasDriverBeenAskedForID)
            {
                Screen.ShowNotification("~r~You have to ask for the ~o~Driver's ID~r~ first!");
                return;
            }

            Helpers.Animations.AnimationRadio();
            Wrappers.Helpers.ShowNotification("Dispatch", $"Running ~o~{IdentityName}", string.Empty);
            await Client.Delay(2000);

            if (!HasRanDriverID)
            {
                HasRanDriverID = true;
                List<string> Offense = new List<string>() { "WANTED BY LSPD", "WANTED FOR ASSAULT", "WANTED FOR UNPAID FINES", "WANTED FOR RUNNING FROM THE POLICE", "WANTED FOR EVADING LAW", "WANTED FOR HIT AND RUN", "WANTED FOR DUI" };

                int OffenseChance = Client.Random.Next(100);

                CanDriverBeArrested = false;
                NotificationFlagOffense = "~g~NONE";

                NotificationFlagCitations = $"{Client.Random.Next(8)}";

                if (OffenseChance >= 75)
                {
                    CanDriverBeArrested = true;
                    NotificationFlagOffense = $"~r~{Offense[Client.Random.Next(Offense.Count)]}";
                }
            }

            Wrappers.Helpers.ShowNotification("Dispatch", $"LSPD Database", $"~w~Name: ~y~{IdentityName}~w~\nGender: ~b~{StoppedDriver.Gender}~w~\nDOB: ~b~{IdentityDateOfBirth}");
            Wrappers.Helpers.ShowNotification("Dispatch", $"LSPD Database", $"~w~Citations: {NotificationFlagCitations}\n~w~Flags: {NotificationFlagOffense}");
        }

        static public async void InteractionHello()
        {
            Helpers.Animations.LoadAnimation("gestures@m@sitting@generic@casual");

            if (Client.speechType == SpeechType.NORMAL)
            {
                string voiceName = "s_f_y_cop_01_white_full_01";
                if (StoppedDriver.Gender == Gender.Male)
                {
                    voiceName = "s_m_y_cop_01_white_full_01";
                }
                PlayAmbientSpeechWithVoice(StoppedDriver.Handle, "KIFFLOM_GREET", voiceName, "SPEECH_PARAMS_FORCE_SHOUTED", false);
                Game.PlayerPed.Task.PlayAnimation("gestures@m@standing@casual", "gesture_hello", 8.0f, -1, (AnimationFlags)49);
            }
            else
            {
                string voiceName = "s_f_y_cop_01_white_full_01";
                if (StoppedDriver.Gender == Gender.Male)
                {
                    voiceName = "s_m_y_cop_01_white_full_01";
                }
                PlayAmbientSpeechWithVoice(StoppedDriver.Handle, "GENERIC_INSULT_HIGH", voiceName, "SPEECH_PARAMS_FORCE_SHOUTED", false);
                Game.PlayerPed.Task.PlayAnimation("gestures@m@standing@casual", "gesture_what_hard", 8.0f, -1, (AnimationFlags)49);
            }
            await Client.Delay(1000);
            Game.PlayerPed.Task.ClearAll();
        }

        static public async void InteractionRunVehicleNumberPlate()
        {
            Helpers.Animations.AnimationRadio();
            Wrappers.Helpers.ShowNotification("Dispatch", $"Running ~o~{TargetVehicle.Mods.LicensePlate}", string.Empty);
            await Client.Delay(2000);
            Wrappers.Helpers.ShowNotification("Dispatch", $"LSPD Database", $"~w~Reg.Owner: ~y~{VehicleRegisterationDriversName}~w~\nReg.Year: ~y~{VehicleRegisterationYear}~w~\nFlags: ~y~{NotificationFlagVehicle}");
        }

        

        static public async void InteractionBreathalyzer()
        {
            if (IsInteractionActive) return;
            IsInteractionActive = true;

            // check ped is in front of the player
            Ped pedInFront = Game.PlayerPed.GetPedInFront();
            Vehicle vehicleInFront = Game.PlayerPed.GetVehicleInFront();
            bool runBreathalyzerChecks = false;
            if (pedInFront != null)
            {
                if (pedInFront.Exists() && pedInFront.IsAlive)
                {
                    // TODO: Add a method for randomly arrested peds also
                    StoppedDriver.Task.TurnTo(Game.PlayerPed, 6000);
                    runBreathalyzerChecks = true;
                }
            }

            if (vehicleInFront != null)
            {
                if (vehicleInFront.Exists())
                {
                    // must be the driver we stopped
                    if (vehicleInFront.Driver == StoppedDriver)
                    {
                        runBreathalyzerChecks = true;
                    }
                }
            }

            if (runBreathalyzerChecks)
            {
                AnimationSearch();
                Wrappers.Helpers.ShowSimpleNotification("~w~Performing ~b~Breathalyzer~w~ test...");
                await Client.Delay(3000);
                string bac = $"~g~0.{DriverBloodAlcaholLimit}";
                if (DriverBloodAlcaholLimit >= 8)
                {
                    CanDriverBeArrested = true;
                    bac = $"~r~0.{DriverBloodAlcaholLimit}";
                }
                Wrappers.Helpers.ShowSimpleNotification($"~b~BAC ~w~Level: {bac}");
            }
            else
            {
                Wrappers.Helpers.ShowSimpleNotification($"~r~Must be facing the suspect.");
            }
            IsInteractionActive = false;
        }

        static public async void InteractionDrugTest()
        {
            if (IsInteractionActive) return;
            IsInteractionActive = true;
            // check ped is in front of the player
            Ped pedInFront = Game.PlayerPed.GetPedInFront();
            Vehicle vehicleInFront = Game.PlayerPed.GetVehicleInFront();
            bool runDrugChecks = false;
            if (pedInFront != null)
            {
                if (pedInFront.Exists() && pedInFront.IsAlive)
                {
                    // TODO: Add a method for randomly arrested peds also
                    StoppedDriver.Task.TurnTo(Game.PlayerPed, 6000);
                    runDrugChecks = true;
                }
            }

            if (vehicleInFront != null)
            {
                if (vehicleInFront.Exists())
                {
                    // must be the driver we stopped
                    if (vehicleInFront.Driver == StoppedDriver)
                    {
                        runDrugChecks = true;
                    }
                }
            }

            if (runDrugChecks)
            {
                AnimationSearch();
                Wrappers.Helpers.ShowSimpleNotification("~w~Performing ~b~Drugalyzer~w~ test...");
                await Client.Delay(3000);
                Wrappers.Helpers.ShowSimpleNotification($"~w~Results:\n~b~  Cannabis~w~: {NotificationFlagCannabis}\n~b~  Cocaine~w~: {NotificationFlagCocaine}");
            }
            else
            {
                Wrappers.Helpers.ShowSimpleNotification($"~r~Must be facing the suspect.");
            }
            IsInteractionActive = false;
        }

        static public async void InteractionSearching()
        {
            if (IsInteractionActive) return;
            IsInteractionActive = true;

            // check ped is in front of the player
            Ped pedInFront = Game.PlayerPed.GetPedInFront();
            Vehicle vehicleInFront = Game.PlayerPed.GetVehicleInFront();
            HasSearchedSuspect = false;

            if (HasSearchedSuspect) return;

            if (pedInFront != null)
            {
                if (pedInFront.Exists() && pedInFront.IsAlive)
                {
                    AnimationSearch();
                    // TODO: Add a method for randomly arrested peds also
                    Wrappers.Helpers.ShowSimpleNotification("~b~Searching~w~ the subject...");
                    HasSearchedSuspect = true;
                }
            }

            if (vehicleInFront != null)
            {
                if (vehicleInFront.Exists())
                {
                    AnimationSearch();
                    Wrappers.Helpers.ShowSimpleNotification("~b~Searching~w~ the vehicle...");
                    HasSearchedSuspect = true;
                    VehicleDoor[] vehicleDoors = TargetVehicle.Doors.GetAll();
                    foreach(VehicleDoor vehicleDoor in vehicleDoors)
                    {
                        vehicleDoor.Open();
                    }
                    await Client.Delay(6000);
                    foreach (VehicleDoor vehicleDoor in vehicleDoors)
                    {
                        vehicleDoor.Close();
                    }
                }
            }

            if (HasSearchedSuspect)
            {
                IsCarryingIllegalItems = false;
                if (CanSearchVehicle)
                {
                    IsCarryingIllegalItems = true;
                    CanDriverBeArrested = true;

                    Wrappers.Helpers.ShowSimpleNotification($"~w~Found ~r~{DataClasses.Police.ItemData.illegalItems[Client.Random.Next(DataClasses.Police.ItemData.illegalItems.Count)]}");
                    if (IsDriverGoingToFleeChance == 9)
                    {
                        SetVehicleCanBeUsedByFleeingPeds(TargetVehicle.Handle, false);
                        StoppedDriver.LeaveGroup();
                        StoppedDriver.Task.ReactAndFlee(Game.PlayerPed);
                    }
                }
                else
                {
                    Wrappers.Helpers.ShowSimpleNotification($"~w~Found ~g~nothing of interest.");
                }
            }
            else
            {
                Wrappers.Helpers.ShowSimpleNotification($"~r~Must be facing the suspect or vehicle.");
            }
            IsInteractionActive = false;
        }

        // Hand in


        static public void ShowRegistration()
        {
            // \n~w~Reg. Year: ~y~{VehicleRegisterationYear}
            Wrappers.Helpers.ShowNotification("Dispatch", "LSPD Database", $"~w~Reg. Owner: ~y~{VehicleRegisterationDriversName}\n~w~Flags: ~y~{NotificationFlagVehicle}");
        }

        static async void ALPR(Vehicle vehicle)
        {
            VehicleHash vehicleHash = (VehicleHash)vehicle.Model.Hash;

            Wrappers.Helpers.ShowNotification("Dispatch", "Getting Information", string.Empty);
            await Client.Delay(0);
            Wrappers.Helpers.ShowNotification("Dispatch", "LSPD Database", $"~w~Plate: ~y~{vehicle.Mods.LicensePlate}~w~\nModel: ~y~{GetLabelText(GetDisplayNameFromVehicleModel((uint)vehicleHash))}~w~\nVehicle Class: {vehicle.ClassType}");
        }

        static async void AnimationTicket()
        {
            string scenario = "CODE_HUMAN_MEDIC_TIME_OF_DEATH";
            if (!Game.PlayerPed.IsInVehicle())
            {
                TaskStartScenarioInPlace(Game.PlayerPed.Handle, scenario, 0, true);
                IsScenarioPlaying = true;
            }
        }

        static async void AnimationSearch()
        {
            string scenario = "PROP_HUMAN_BUM_BIN";
            if (!Game.PlayerPed.IsInVehicle())
            {
                TaskStartScenarioInPlace(Game.PlayerPed.Handle, scenario, 0, true);
                await Client.Delay(5000);
                Game.PlayerPed.Task.ClearAll();
            }
        }
    }
}

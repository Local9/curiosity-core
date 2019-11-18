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
using Curiosity.Shared.Client.net.Enums.Patrol;

namespace Curiosity.Missions.Client.net.Scripts.Police
{
    class TrafficStop
    {
        static Client client = Client.GetInstance();
        static bool IsScenarioPlaying = false;

        static float DistanceToCheck = 10.0f;

        static public Vehicle TargetVehicle;
        static public Ped StoppedDriver;
        static bool AwaitingPullover = true;
        static bool IsConductingPullover = false;

        // states
        static bool IsVehicleFleeing = false;
        static bool IsVehicleStopped = false;
        // states for menu
        static bool CanSearchVehicle = false;
        static bool IsDriverUnderTheInfluence = false;
        static bool HasDriverBeenAskedForID = false;
        static bool IsVehicleDriverMimicking = false;
        static bool IsVehicleDriverFollowing = false;
        static bool IsDriverFollowing = false;
        static bool HasVehicleBeenStolen = false;
        static bool CanDriverBeArrested = false;

        // Ped Data
        static int DriverBloodAlcaholLimit;
        static int DriverLostIdChance;
        static int DriverAttitude;

        static int DriverCannabisChance;
        static int DriverCocaineChance;

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
            if (Classes.PlayerClient.ClientInformation.IsDeveloper())
            {
                client.RegisterTickHandler(OnTrafficStopTask);
                client.RegisterTickHandler(OnTrafficStopStateTask);
                client.RegisterTickHandler(OnEmoteCheck);
                Screen.ShowNotification("~b~Traffic Stops~s~: ~g~Enabled");
            }
        }

        public static void Dispose()
        {
            if (Classes.PlayerClient.ClientInformation.IsDeveloper())
            {
                client.DeregisterTickHandler(OnTrafficStopTask);
                client.DeregisterTickHandler(OnTrafficStopStateTask);
                client.DeregisterTickHandler(OnEmoteCheck);
                Screen.ShowNotification("~b~Traffic Stops~s~: ~r~Disabled");
            }
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

                                if (Game.IsControlJustReleased(0, Control.Pickup))
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
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnTrafficStopTask -> {ex}");
            }
        }

        internal static async void InteractionRelease()
        {
            if (Client.speechType == SpeechType.NORMAL)
            {
                ShowOfficerSubtitle("Alright, you're free to go.");
            }
            else
            {
                ShowOfficerSubtitle("Get out of here before I change my mind.");
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
            ShowDriverSubtitle(DriverResponse[Client.Random.Next(DriverResponse.Count)]);
            Reset();
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

            ShowOfficerSubtitle(OfficerResponse[Client.Random.Next(OfficerResponse.Count)]);
            await Client.Delay(2000);
            ShowDriverSubtitle(DriverResponse[Client.Random.Next(DriverResponse.Count)]);
            await Client.Delay(2000);
            Reset();
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

                if (IsVehicleStopped && TargetVehicle.IsEngineRunning && !IsVehicleDriverMimicking)
                {
                    TargetVehicle.IsEngineRunning = false;

                    if (TargetVehicle.Speed <= 1f)
                    {
                        TargetVehicle.Windows.RollDownAllWindows();
                    }
                }

                if (IsVehicleStopped && Game.PlayerPed.IsInVehicle() && !IsVehicleDriverMimicking)
                {
                    Screen.DisplayHelpTextThisFrame("Press ~INPUT_COVER~ to move the ~b~Vehicle");
                }

                if (IsVehicleStopped && Game.PlayerPed.IsInVehicle() && IsVehicleDriverMimicking)
                {
                    Screen.DisplayHelpTextThisFrame("Press ~INPUT_COVER~ to ~b~stop moving the Vehicle");
                }

                if (TargetVehicle.Position.Distance(Game.PlayerPed.Position) >= 300f)
                {
                    if (TargetVehicle.AttachedBlip != null)
                        ShowNotification("Dispatch", "~r~They got away...", string.Empty);

                    Reset();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnTrafficStopStateTask -> {ex}");
            }
        }

        static public void Reset()
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
                StoppedDriver.IsPersistent = false;
            }

            IsVehicleStopped = false;
            IsVehicleFleeing = false;

            IsConductingPullover = false;

            TargetVehicle = null;

            NotificationFlagVehicle = "~g~NONE";
            NotificationFlagCannabis = "~g~Negative";
            NotificationFlagCocaine = "~g~Negative";

            HasDriverBeenAskedForID = false;
            IsDriverUnderTheInfluence = false;
            CanSearchVehicle = false;
            HasVehicleBeenStolen = false;

            client.DeregisterTickHandler(MenuHandler.SuspectMenu.OnMenuTask);
            API.TaskSetBlockingOfNonTemporaryEvents(StoppedDriver.Handle, false);
        }

        static async void Pullover(Vehicle stoppedVehicle)
        {
            IsConductingPullover = true; // Flag that a pullover has started

            StoppedDriver = stoppedVehicle.Driver;
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
                NotificationFlagCannabis = "~r~Positive";
                DriverChanceOfFlee = Client.Random.Next(18, 30);
            }

            if (DriverCocaineChance > 90)
            {
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
                    bool IsDriverGoingToFlee = true;
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

            ped.Weapons.Give(WeaponHash.CombatPistol, 30, false, true);
            ped.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
            await Client.Delay(1000);
            ped.Task.ShootAt(Game.PlayerPed, 10000000, FiringPattern.FullAuto);
        }

        static async void TrafficStopVehicleFlee(Vehicle vehicle, Ped ped)
        {
            try
            {
                API.TaskSetBlockingOfNonTemporaryEvents(StoppedDriver.Handle, false);
                
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

        static public async void InteractionRequestPedIdentification()
        {
            if (StoppedDriver == null)
            {
                ShowNotification("No No No", "Traffic Stop Required", string.Empty, NotificationCharacter.CHAR_LESTER);
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

                    ShowNotification("Driver's ID", string.Empty, $"~w~Name: ~y~{IdentityName}\n~w~DOB: ~y~{IdentityDateOfBirth}");
                    
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
            AnimationRadio();
            ShowNotification("Dispatch", $"Running ~o~{IdentityName}", string.Empty);
            await Client.Delay(2000);
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

            ShowNotification("Dispatch", $"LSPD Database", $"~w~Name: ~y~{IdentityName}~w~\nGender: ~b~{StoppedDriver.Gender}~w~\nDOB: ~b~{IdentityDateOfBirth}");
            ShowNotification("Dispatch", $"LSPD Database", $"~w~Citations: {NotificationFlagCitations}\n~w~Flags: {NotificationFlagOffense}");
        }

        static public async void InteractionHello()
        {
            LoadAnimation("gestures@m@sitting@generic@casual");

            if (Client.speechType == SpeechType.NORMAL)
            {
                PlayAmbientSpeechWithVoice(StoppedDriver.Handle, "KIFFLOM_GREET", "s_m_y_sheriff_01_white_full_01", "SPEECH_PARAMS_FORCE_SHOUTED", false);
                Game.PlayerPed.Task.PlayAnimation("gestures@m@standing@casual", "gesture_hello", 8.0f, -1, (AnimationFlags)49);
            }
            else
            {
                PlayAmbientSpeechWithVoice(StoppedDriver.Handle, "GENERIC_INSULT_HIGH", "s_m_y_sheriff_01_white_full_01", "SPEECH_PARAMS_FORCE_SHOUTED", false);
                Game.PlayerPed.Task.PlayAnimation("gestures@m@standing@casual", "gesture_what_hard", 8.0f, -1, (AnimationFlags)49);
            }
            await Client.Delay(1000);
            Game.PlayerPed.Task.ClearAll();
        }

        static public async void InteractionRunVehicleNumberPlate()
        {
            AnimationRadio();
            ShowNotification("Dispatch", $"Running ~o~{TargetVehicle.Mods.LicensePlate}", string.Empty);
            await Client.Delay(2000);
            ShowNotification("Dispatch", $"LSPD Database", $"~w~Reg.Owner: ~y~{VehicleRegisterationDriversName}~w~\nReg.Year: ~y~{VehicleRegisterationYear}~w~\nFlags: ~y~{NotificationFlagVehicle}");
        }

        static public void InteractionDrunk()
        {
            ShowOfficerSubtitle("Have you had anything to drink today?");
            List<string> response;
            if (IsDriverUnderTheInfluence)
            {
                response = new List<string>() { "*Burp*", "What's a drink?", "No.", "You'll never catch me alive!", "Never", "Nope, i don't drink Ossifer", "Maybe?", "Just a few." };
            }
            else
            {
                response = new List<string>() { "No, sir", "I dont drink.", "Nope.", "No.", "Only 1.", "Yes... a water and 2 orange juices." };
            }
            ShowDriverSubtitle(response[Client.Random.Next(response.Count)]);
        }

        static public void InteractionDrug()
        {
            ShowOfficerSubtitle("Have you consumed any drugs recently?");
            List<string> response;
            if (DriverCocaineChance >= 90 || DriverCannabisChance >= 85)
            {
                response = new List<string>() { "What is life?", "Who is me?", "NoOOOooo.", "Is that a UNICORN?!", "If I've done the what?", "WHAT DRUGS? I DONT KNOW KNOW ANYTHING ABOUT DRUGS.", "What's a drug?" };
            }
            else
            {
                response = new List<string>() { "No, sir", "I don't do that stuff.", "Nope.", "No.", "Nah" };
            }
            ShowDriverSubtitle(response[Client.Random.Next(response.Count)]);
        }

        static public void InteractionIllegal()
        {
            ShowOfficerSubtitle("Is there anything illegal in the vehicle?");
            List<string> response = new List<string>() { "No, sir", "Not that I know of.", "Nope.", "No.", "Apart from the 13 dead hookers in the back.. No.", "Maybe? But most probably not.", "I sure hope not" };
            ShowDriverSubtitle(response[Client.Random.Next(response.Count)]);
        }

        static public void InteractionSearch()
        {
            ShowOfficerSubtitle("Would you mind if i search your vehicle?");
            List<string> response;
            if (CanSearchVehicle)
            {
                response = new List<string>() { "I'd prefer you not to...", "I'll have to pass on that", "Uuuh... Y- No..", "Go ahead. For the record its not my car", "Yeah, why not.." };
            }
            else
            {
                response = new List<string>() { "Go ahead", "Shes all yours", "I'd prefer you not to", "I don't have anything to hide, go for it." };
            }
            ShowDriverSubtitle(response[Client.Random.Next(response.Count)]);
        }

        static void ShowOfficerSubtitle(string subtitle)
        {
            Screen.ShowSubtitle($"~o~Officer:~w~ {subtitle}");
        }

        static void ShowDriverSubtitle(string subtitle)
        {
            Screen.ShowSubtitle($"~b~Driver:~w~ {subtitle}");
        }

        static public void ShowRegistration()
        {
            // \n~w~Reg. Year: ~y~{VehicleRegisterationYear}
            ShowNotification("Dispatch", "LSPD Database", $"~w~Reg. Owner: ~y~{VehicleRegisterationDriversName}\n~w~Flags: ~y~{NotificationFlagVehicle}");
        }

        static async void ALPR(Vehicle vehicle)
        {
            VehicleHash vehicleHash = (VehicleHash)vehicle.Model.Hash;

            ShowNotification("Dispatch", "Getting Information", string.Empty);
            await Client.Delay(0);
            ShowNotification("Dispatch", "LSPD Database", $"~w~Plate: ~y~{vehicle.Mods.LicensePlate}~w~\nModel: ~y~{GetLabelText(GetDisplayNameFromVehicleModel((uint)vehicleHash))}~w~\nVehicle Class: {vehicle.ClassType}");
        }

        static void ShowNotification(string title, string subtitle, string message, NotificationCharacter notificationCharacter = NotificationCharacter.CHAR_CALL911)
        {
            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{notificationCharacter}", 2, title, subtitle, message, 2);
        }

        static async void AnimationRadio()
        {
            LoadAnimation("random@arrests");
            Game.PlayerPed.Task.PlayAnimation("random@arrests", "generic_radio_enter", 1.5f, 2.0f, -1, (AnimationFlags)50, 2.0f);
            await Client.Delay(6000);
            Game.PlayerPed.Task.ClearAll();
        }

        static async void AnimationTicket()
        {
            string ticketScenario = "CODE_HUMAN_MEDIC_TIME_OF_DEATH";
            if (!Game.PlayerPed.IsInVehicle())
            {
                TaskStartScenarioInPlace(Game.PlayerPed.Handle, ticketScenario, 0, true);
                IsScenarioPlaying = true;
            }
        }

        static async Task<bool> LoadAnimation(string dict)
        {
            while (!HasAnimDictLoaded(dict))
            {
                await Client.Delay(0);
                RequestAnimDict(dict);
            }
            return true;
        }
    }
}

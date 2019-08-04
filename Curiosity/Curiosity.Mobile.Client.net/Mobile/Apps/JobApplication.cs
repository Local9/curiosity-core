using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Mobile.Client.net.Mobile.Api;
using Curiosity.Global.Shared.net.Enums.Mobile;
using Curiosity.Global.Shared.net.Enums;
using Curiosity.Shared.Client.net.Enums.Patrol;
using System;
using System.Threading.Tasks;

namespace Curiosity.Mobile.Client.net.Mobile.Apps
{
    class JobApplication
    {
        static Application App;
        static string PREFIX = "~l~";
        static string CurrentJob = string.Empty;
        static long GameTimer = API.GetGameTimer();
        static bool timeout = false;
        static PatrolZone PatrolZone = PatrolZone.City;

        public static void Init()
        {
            App = new Application("Job Options", Api.AppIcons.Tasks, 2);
            Screen screen = new Screen(App, "Job Options", (int)View.Settings);
            App.LauncherScreen = screen;
            App.StartTask = StartTick;
            ApplicationHandler.Apps.Add(App); // Add the app to the loaded apps.
        }

        public static bool StartTick()
        {
            Tick();
            return true;
        }

        public static bool StopTick()
        {
            return true;
        }

        public static void Tick()
        {
            if (App != null && ApplicationHandler.CurrentApp == App)
            {
                App.LauncherScreen.ClearItems();
                App.LauncherScreen.Items.Clear();

                // SETUP OPTIONS
                Screen jobMenu = App.AddScreenType("Job", View.Settings);
                //jobMenu.AddItem(new Item(jobMenu, Item.CreateData(2, PREFIX + $"{Job.Firefighter}", (int)ListIcons.Settings1), SetJob, Job.Firefighter));
                //jobMenu.AddItem(new Item(jobMenu, Item.CreateData(2, PREFIX + $"{Job.Paramedic}", (int)ListIcons.Settings1), SetJob, Job.Paramedic));
                //jobMenu.AddItem(new Item(jobMenu, Item.CreateData(2, PREFIX + $"{Job.Pilot}", (int)ListIcons.Settings1), SetJob, Job.Pilot));
                jobMenu.AddItem(new Item(jobMenu, Item.CreateData(2, PREFIX + $"{Job.PoliceOfficer}", (int)ListIcons.Settings1), SetJob, Job.PoliceOfficer));
                //jobMenu.AddItem(new Item(jobMenu, Item.CreateData(2, PREFIX + $"{Job.Trucker}", (int)ListIcons.Settings1), SetJob, Job.Trucker));

                // ADD TO SCREEN
                App.LauncherScreen.AddItem(new Item(App.LauncherScreen, Item.CreateData(2, PREFIX + "Job", (int)ListIcons.Settings1), ApplicationHandler.ChangeScreen, jobMenu));
                App.LauncherScreen.AddItem(new Item(App.LauncherScreen, Item.CreateData(2, PREFIX + $"Change Status", (int)ListIcons.Settings1), ToggleActiveStatus));
                App.LauncherScreen.AddItem(new Item(App.LauncherScreen, Item.CreateData(2, PREFIX + $"Change Location", (int)ListIcons.Settings1), ToggleLocation));
                GameTimer = API.GetGameTimer();
            }
        }

        static void SetJob(dynamic[] dynamics)
        {
            MobilePhone.IsJobActive = false;

            Job selectedJob = (Job)dynamics[0];
            CurrentJob = string.Empty;

            switch (selectedJob)
            {
                case Job.Firefighter:
                    CurrentJob = "fire";
                    break;
                case Job.Paramedic:
                    CurrentJob = "medic";
                    break;
                case Job.PoliceOfficer:
                    CurrentJob = "police";
                    break;
                default:
                    CurrentJob = string.Empty;
                    break;
            }

            API.SetDiscordRichPresenceAssetSmall("banner");
            API.SetDiscordRichPresenceAssetSmallText($"{selectedJob}");

            Client.TriggerEvent("curiosity:Client:Interface:Duty", !string.IsNullOrEmpty(CurrentJob), MobilePhone.IsJobActive, CurrentJob);
            Client.TriggerEvent("curiosity:Mobile:Job:Active", MobilePhone.IsJobActive);
        }

        static void ToggleLocation(dynamic[] dynamics)
        {
            if (string.IsNullOrEmpty(CurrentJob))
            {
                API.PlaySoundFrontend(-1, "ERROR", "HUD_AMMO_SHOP_SOUNDSET", true);
                Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Job Status", "", $"~w~Status cannot be changed, please select a job first.", 2);
                return;
            }

            PatrolZone = (PatrolZone == PatrolZone.City) ? PatrolZone.Country : PatrolZone.City;
            Client.TriggerEvent("curiosity:Client:Police:PatrolZone", (int)PatrolZone);
        }

        static async void ToggleActiveStatus(dynamic[] dynamics)
        {
            if(string.IsNullOrEmpty(CurrentJob))
            {
                API.PlaySoundFrontend(-1, "ERROR", "HUD_AMMO_SHOP_SOUNDSET", true);
                Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Job Status", "", $"~w~Status cannot be changed, please select a job first.", 2);
                return;
            }

            if (timeout)
            {
                API.PlaySoundFrontend(-1, "ERROR", "HUD_AMMO_SHOP_SOUNDSET", true);
                Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Job Status", "", $"~w~Status cannot be changed currently, please wait.", 2);
                return;
            }
            MobilePhone.IsJobActive = !MobilePhone.IsJobActive;
            Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Job Status", $"~w~Status: ~s~{(MobilePhone.IsJobActive ? $"Active" : $"Deactivated")}", string.Empty, 2);
            Client.TriggerEvent("curiosity:Client:Interface:Duty", !string.IsNullOrEmpty(CurrentJob), MobilePhone.IsJobActive, CurrentJob);
            await TimeOut();
        }

        static async Task TimeOut()
        {
            timeout = true;
            GameTimer = API.GetGameTimer();
            while ((API.GetGameTimer() - GameTimer) < 10000)
            {
                await Client.Delay(1000);
            }
            timeout = false;

            API.PlaySoundFrontend(-1, "CHALLENGE_UNLOCKED", "HUD_AWARDS", true);
            Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Job Status", $"~w~Status can now be changed.", string.Empty, 2);
        }
    }
}

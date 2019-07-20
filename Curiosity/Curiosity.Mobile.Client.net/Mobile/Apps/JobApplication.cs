using CitizenFX.Core.Native;
using Curiosity.Mobile.Client.net.Mobile.Api;
using Curiosity.Global.Shared.net.Enums.Mobile;
using Curiosity.Global.Shared.net.Enums;
using System;
using System.Threading.Tasks;

namespace Curiosity.Mobile.Client.net.Mobile.Apps
{
    class JobApplication
    {
        static Application App;
        static string PREFIX = "~l~";

        public static void Init()
        {
            App = new Application("Job Options", Api.AppIcons.Tasks, 2);
            Screen screen = new Screen(App, "Job Options", (int)View.Settings);
            App.LauncherScreen = screen;
            App.StartTask = StartTick;
            App.StopTask = StopTick;
            ApplicationHandler.Apps.Add(App); // Add the app to the loaded apps.
        }

        public static bool StartTick()
        {
            Client.GetInstance().RegisterTickHandler(Tick);
            return true;
        }

        public static bool StopTick()
        {
            Client.GetInstance().DeregisterTickHandler(Tick);
            return true;
        }

        public static async Task Tick()
        {
            if (App != null && ApplicationHandler.CurrentApp == App)
            {
                App.LauncherScreen.ClearItems();
                App.LauncherScreen.Items.Clear();

                // SETUP OPTIONS
                Screen jobMenu = App.AddScreenType("Job", View.Settings);
                jobMenu.AddItem(new Item(jobMenu, Item.CreateData(2, PREFIX + $"{Job.Firefighter}", (int)ListIcons.Settings1), SetJob, Job.Firefighter));
                jobMenu.AddItem(new Item(jobMenu, Item.CreateData(2, PREFIX + $"{Job.Paramedic}", (int)ListIcons.Settings1), SetJob, Job.Paramedic));
                jobMenu.AddItem(new Item(jobMenu, Item.CreateData(2, PREFIX + $"{Job.Pilot}", (int)ListIcons.Settings1), SetJob, Job.Pilot));
                jobMenu.AddItem(new Item(jobMenu, Item.CreateData(2, PREFIX + $"{Job.PoliceOfficer}", (int)ListIcons.Settings1), SetJob, Job.PoliceOfficer));
                jobMenu.AddItem(new Item(jobMenu, Item.CreateData(2, PREFIX + $"{Job.Trucker}", (int)ListIcons.Settings1), SetJob, Job.Trucker));

                // ADD TO SCREEN
                App.LauncherScreen.AddItem(new Item(App.LauncherScreen, Item.CreateData(2, PREFIX + $"{(MobilePhone.IsOnDuty ? "Come off Duty" : "Go on Duty")}", (int)ListIcons.Settings1), ToggleDutyStatus));
                App.LauncherScreen.AddItem(new Item(App.LauncherScreen, Item.CreateData(2, PREFIX + "Job", (int)ListIcons.Settings1), ApplicationHandler.ChangeScreen, jobMenu));
            }
            await Task.FromResult(0);
        }

        static void SetJob(dynamic[] dynamics)
        {

        }

        static void ToggleDutyStatus(dynamic[] dynamics)
        {
            MobilePhone.IsOnDuty = !MobilePhone.IsOnDuty;
            Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Job Status", "Changed Duty", $"~w~Duty Status: ~s~{(MobilePhone.IsOnDuty ? $"On Duty" : $"Off Duty")}", 2);
            ApplicationHandler.Kill();
        }
    }
}

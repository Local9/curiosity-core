using Curiosity.Global.Shared.net.Entity;
using Curiosity.Global.Shared.net.Enums;
using Curiosity.Global.Shared.net.Enums.Mobile;
using Curiosity.Mobile.Client.net.Mobile.Api;
using System;

namespace Curiosity.Mobile.Client.net.Mobile.Apps
{
    class SkillsApplication
    {
        static Application App;
        static string PREFIX = "~l~";

        static Client client = Client.GetInstance();

        static Screen SkillScreen;
        static Screen StatScreen;

        public static void Init()
        {
            App = new Application("Skills and Stats", Api.AppIcons.Games, 4);
            Screen screen = new Screen(App, "Skills and Stats", View.Settings);
            App.LauncherScreen = screen;
            App.StartTask = StartTick;
            ApplicationHandler.Apps.Add(App); // Add the app to the loaded apps.
            client.RegisterEventHandler("curiosity:Player:Skills:GetListData", new Action<string>(OpenDataList));
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
                SkillScreen = App.AddScreenType("Skills", View.JobList);
                StatScreen = App.AddScreenType("Stats", View.JobList);

                SkillScreen.AddItem(new Item(SkillScreen, Item.CreateData($"{PREFIX}Loading...", $"Please Wait", 0, 1, 1), null, null));
                StatScreen.AddItem(new Item(StatScreen, Item.CreateData($"{PREFIX}Loading...", $"Please Wait", 0, 1, 1), null, null));

                App.LauncherScreen.AddItem(new Item(App.LauncherScreen, Item.CreateData(2, PREFIX + "Skills", (int)ListIcons.Profile), GetDataForScreen, SkillScreen, 0));
                App.LauncherScreen.AddItem(new Item(App.LauncherScreen, Item.CreateData(2, PREFIX + "Stats", (int)ListIcons.Profile), GetDataForScreen, StatScreen, 1));
            }
        }

        static void GetList(int skill)
        {
            Client.TriggerServerEvent("curiosity:Server:Skills:GetListData", skill);
        }

        static async void GetDataForScreen(dynamic[] data)
        {
            ApplicationHandler.ChangeScreen(data);

            if (data[1] == 1)
            {
                GetList((int)SkillType.Statistic);
            }
            else
            {
                GetList((int)SkillType.Experience);
            }
            await Client.Delay(0);
        }

        static async void OpenDataList(string listdata)
        {
            if (App != null && ApplicationHandler.CurrentApp == App)
            {
                NuiData nuiData = Newtonsoft.Json.JsonConvert.DeserializeObject<NuiData>(listdata);
                SkillType skillType = (SkillType)Enum.Parse(typeof(SkillType), nuiData.panel, true);

                if (skillType == SkillType.Experience)
                {
                    SkillScreen.ClearItems();

                    if (nuiData.skills.Count == 0)
                    {
                        SkillScreen.AddItem(new Item(SkillScreen, Item.CreateData($"{PREFIX}No Skills", $"Time to get to work aye?", 0, 1, 1), null, null));
                        return;
                    }

                    foreach (Skills skills in nuiData.skills)
                    {
                        SkillScreen.AddItem(new Item(SkillScreen, Item.CreateData($"{PREFIX}{skills.Label}", $"{skills.Value:00}", 0, 1, 1), null, null));
                        await Client.Delay(0);
                    }
                }
                else
                {
                    StatScreen.ClearItems();

                    if (nuiData.skills.Count == 0)
                    {
                        StatScreen.AddItem(new Item(StatScreen, Item.CreateData($"{PREFIX}No Stats", $"Time to get to work aye?", 0, 1, 1), null, null));
                        return;
                    }

                    foreach (Skills skills in nuiData.skills)
                    {
                        StatScreen.AddItem(new Item(StatScreen, Item.CreateData($"{PREFIX}{skills.Label}", $"{skills.Value:00}", 0, 1, 1), null, null));
                        await Client.Delay(0);
                    }
                }
            }
            await Client.Delay(0);
        }
    }
}

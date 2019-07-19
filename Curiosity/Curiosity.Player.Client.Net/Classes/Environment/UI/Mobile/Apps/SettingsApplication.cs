using CitizenFX.Core.Native;
using Curiosity.Client.net.Classes.Environment.UI.Mobile.Api;
using Curiosity.Global.Shared.net.Enums.Mobile;
using System;

namespace Curiosity.Client.net.Classes.Environment.UI.Mobile.Apps
{
    class SettingsApplication
    {
        static Application App;
        static string PREFIX = "~l~";

        public static void Init()
        {
            App = new Application("Settings", Api.AppIcons.Settings, 2);
            Screen screen = new Screen(App, "Settings", (int)View.Settings);

            screen.AddItem(new Item(screen, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_820"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetTheme), 1));
            screen.AddItem(new Item(screen, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_821"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetTheme), 2));
            screen.AddItem(new Item(screen, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_822"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetTheme), 3));
            screen.AddItem(new Item(screen, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_823"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetTheme), 4));
            screen.AddItem(new Item(screen, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_824"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetTheme), 5));
            screen.AddItem(new Item(screen, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_825"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetTheme), 6));
            screen.AddItem(new Item(screen, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_826"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetTheme), 7));

            App.LauncherScreen = screen;
            ApplicationHandler.Apps.Add(App); // Add the app to the loaded apps.
        }

        public static void SetTheme(dynamic[] theme)
        {
            MobilePhone.phoneTheme = theme[0];
            API.SetResourceKvpInt("vf_phone_theme", theme[0]);
        }

        public static void SetWallpaper(dynamic[] dynamics)
        {
            MobilePhone.phoneWallpaper = dynamics[0];
            API.SetResourceKvpInt("vf_phone_wallpaper", dynamics[0]);
        }
    }
}

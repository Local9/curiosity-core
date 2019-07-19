using CitizenFX.Core.Native;
using Curiosity.Mobile.Client.net.Mobile.Api;
using Curiosity.Global.Shared.net.Enums.Mobile;
using System;

namespace Curiosity.Mobile.Client.net.Mobile.Apps
{
    class SettingsApplication
    {
        static Application App;
        static string PREFIX = "~l~";

        public static void Init()
        {
            App = new Application("Settings", Api.AppIcons.Settings, 7);
            Screen screen = new Screen(App, "Settings", (int)View.Settings);
            Screen theme = App.AddScreenType("Theme", View.Settings);
            Screen wallpaper = App.AddScreenType("Wallpaper", View.Settings);

            App.LauncherScreen = screen;

            screen.AddItem(new Item(App.LauncherScreen, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_720"), (int)ListIcons.Settings1), ApplicationHandler.ChangeScreen, theme));
            screen.AddItem(new Item(App.LauncherScreen, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_740"), (int)ListIcons.Settings1), ApplicationHandler.ChangeScreen, wallpaper));

            theme.AddItem(new Item(theme, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_820"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetTheme), 1));
            theme.AddItem(new Item(theme, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_821"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetTheme), 2));
            theme.AddItem(new Item(theme, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_822"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetTheme), 3));
            theme.AddItem(new Item(theme, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_823"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetTheme), 4));
            theme.AddItem(new Item(theme, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_824"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetTheme), 5));
            theme.AddItem(new Item(theme, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_825"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetTheme), 6));
            theme.AddItem(new Item(theme, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_826"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetTheme), 7));

            wallpaper.AddItem(new Item(wallpaper, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_844"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetWallpaper), 4));
            wallpaper.AddItem(new Item(wallpaper, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_845"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetWallpaper), 5));
            wallpaper.AddItem(new Item(wallpaper, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_846"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetWallpaper), 6));
            wallpaper.AddItem(new Item(wallpaper, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_847"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetWallpaper), 7));
            wallpaper.AddItem(new Item(wallpaper, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_848"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetWallpaper), 8));
            wallpaper.AddItem(new Item(wallpaper, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_849"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetWallpaper), 9));
            wallpaper.AddItem(new Item(wallpaper, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_850"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetWallpaper), 10));
            wallpaper.AddItem(new Item(wallpaper, Item.CreateData(2, PREFIX + API.GetLabelText("CELL_851"), (int)ListIcons.Settings1), new Action<dynamic[]>(SetWallpaper), 11));

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

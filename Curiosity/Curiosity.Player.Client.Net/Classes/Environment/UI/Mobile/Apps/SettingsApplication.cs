using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using Curiosity.Client.net.Classes.Environment.UI.Mobile.Api;

namespace Curiosity.Client.net.Classes.Environment.UI.Mobile.Apps
{
    class SettingsApplication
    {
        static Application App;

        public static void Init()
        {
            App = new Application("Settings", Api.AppIcons.Settings);
            Screen screen = new Screen(App, "Settings", Screen.LIST);
            App.LauncherScreen = screen;
            ApplicationHandler.Apps.Add(App); // Add the app to the loaded apps.
        }

        public static void SetTheme(int theme)
        {
            MobilePhone.phoneTheme = theme;
            API.SetResourceKvpInt("vf_phone_theme", theme);
        }

        public static void SetWallpaper(int wallpaper)
        {
            MobilePhone.phoneWallpaper = wallpaper;
            API.SetResourceKvpInt("vf_phone_wallpaper", wallpaper);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curiosity.Client.net.Classes.Environment.UI.Mobile.Api;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Client.net.Classes.Environment.UI.Mobile.Apps
{
    class PlayerListApplication
    {
        static Application App;
        static string PREFIX = "~l~";

        // public static Action<dynamic[]> CHANGE_THEME = new Action<dynamic[]>(ApplicationSettings.SetTheme);
        // public static Action<dynamic[]> CHANGE_WALLPAPER = new Action<dynamic[]>(ApplicationSettings.SetWallpaper);
        // public static Action<dynamic[]> CHANGE_TEXT_COLOR = new Action<dynamic[]>(ChangeTextColor);

        public static void Init()
        {
            App = new Application("Player List", AppIcons.ContactsPlus);
            Screen screen = new Screen(App, "Player List", Screen.LIST);
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

        // OnTick for the Player List application.
        public static async Task Tick()
        {
            if (App != null && ApplicationHandler.CurrentApp == App)
            {
                App.LauncherScreen.ClearItems();
                /*for (int i = 0; i < PlayerList.MaxPlayers; i++)
                {
                }*/
                foreach (CitizenFX.Core.Player player in Client.players)
                {
                    /* Skip the current client.
                    if (player == LocalPlayer)
                    {
                        continue;
                    }*/
                    Screen playerOptionsMenu = App.AddListScreen(player.Name); // Screen which contains options for this player.
                    Item playerItem = new Item(App.LauncherScreen, // Create an item that switches to the new screen.
                        Item.CreateData(0, PREFIX + player.Name), // Item push data.
                        ApplicationHandler.ChangeScreen, playerOptionsMenu); // Action callback and arguments.
                    App.LauncherScreen.AddItem(playerItem); // Add the item to the launcher screen.

                    // Add option to message the player.
                    Item messageItem = new Item(playerOptionsMenu, Item.CreateData(0, PREFIX + "Message 1"),
                        new Action<dynamic[]>(MessagePlayer), player.ServerId);

                    Item messageItem2 = new Item(playerOptionsMenu, Item.CreateData(0, PREFIX + "Message 2"),
                        new Action<dynamic[]>(MessagePlayer), player.ServerId);

                    playerOptionsMenu.AddItem(messageItem);
                    playerOptionsMenu.AddItem(messageItem2);
                }
                await BaseScript.Delay(1000); // Tick occurs once per second.
            }
        }

        static async void MessagePlayer(dynamic[] data)
        {
            API.DisplayOnscreenKeyboard(6, "FMMC_KEY_TIP8", "", "", "", "", "", 60);

            await BaseScript.Delay(100); // 100ms delay to prevent instant enter.

            while (API.UpdateOnscreenKeyboard() == 0)
            {
                API.DisableAllControlActions(0);
                await BaseScript.Delay(0);
            }

            if (API.UpdateOnscreenKeyboard() == 1)
            {
                string message = API.GetOnscreenKeyboardResult();
                API.SetNotificationTextEntry("STRING");
                if (message.Length <= 0)
                {
                    API.AddTextComponentString("~r~Message too short!");
                }
                else
                {
                    API.AddTextComponentString("~g~Message sent! " + message);
                }
                API.DrawNotification(true, true);
            }
            API.EnableAllControlActions(0);
        }
    }
}

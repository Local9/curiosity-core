using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curiosity.Mobile.Client.net.Mobile.Api;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net.Enums.Mobile;

namespace Curiosity.Mobile.Client.net.Mobile.Apps
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
            App = new Application("Player List", AppIcons.Contacts, 0);
            Screen screen = new Screen(App, "Player List", (int)View.Contacts);
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
                App.LauncherScreen.Items.Clear();

                foreach (CitizenFX.Core.Player player in Client.players)
                {
                    /* Skip the current client.*/
                    if (player == Game.Player)
                    {
                        continue;
                    }
                    Screen playerOptionsMenu = App.AddScreenType(player.Name, View.Settings); // Screen which contains options for this player.

                    // Add option to message the player.
                    
                    Item messageItem = new Item(playerOptionsMenu, Item.CreateData(2, PREFIX + "Message", (int)ListIcons.Attachment),
                        MessagePlayer, player);

                    playerOptionsMenu.AddItem(messageItem);

                    Item playerItem = new Item(App.LauncherScreen, // Create an item that switches to the new screen.
                        Item.CreateData(2, PREFIX + player.Name, 1), // Item push data.
                        ApplicationHandler.ChangeScreen, playerOptionsMenu); // Action callback and arguments.
                    App.LauncherScreen.AddItem(playerItem); // Add the item to the launcher screen.
                }
                await BaseScript.Delay(1000); // Tick occurs once per second.
            }
        }

        static async void MessagePlayer(dynamic[] data)
        {
            Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "SMS", "Message Error", "Network Error", 2);
            ApplicationHandler.Kill();
            await BaseScript.Delay(0);

            //ApplicationHandler.IsInKeyboard = true;

            //await BaseScript.Delay(100);

            //API.DisplayOnscreenKeyboard(6, "FMMC_KEY_TIP8", "", "", "", "", "", 60);

            //await BaseScript.Delay(100); // 100ms delay to prevent instant enter.

            //while (API.UpdateOnscreenKeyboard() != 1 && API.UpdateOnscreenKeyboard() != 2)
            //{
            //    API.DisableAllControlActions(0);
            //    await BaseScript.Delay(0);
            //}

            //if (API.UpdateOnscreenKeyboard() == 1)
            //{
            //    string message = API.GetOnscreenKeyboardResult();
            //    if (message.Length <= 0)
            //    {
            //        Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "SMS", "Message Error", "Message was too short", 2);
            //    }
            //    else
            //    {
            //        Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "SMS", "Message Sent", $"To: {data[0].Name}~n~{message}", 2);
            //    }
            //}
            //API.EnableAllControlActions(0);
            //ApplicationHandler.IsInKeyboard = false;
        }
    }
}

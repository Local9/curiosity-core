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

        static Screen screen;

        // public static Action<dynamic[]> CHANGE_THEME = new Action<dynamic[]>(ApplicationSettings.SetTheme);
        // public static Action<dynamic[]> CHANGE_WALLPAPER = new Action<dynamic[]>(ApplicationSettings.SetWallpaper);
        // public static Action<dynamic[]> CHANGE_TEXT_COLOR = new Action<dynamic[]>(ChangeTextColor);

        public static void Init()
        {

            App = new Application("Contacts", AppIcons.Contacts, 0);
            screen = new Screen(App, "Contacts", (int)View.Contacts);
            App.LauncherScreen = screen;
            App.StartTask = StartTick;
            ApplicationHandler.Apps.Add(App); // Add the app to the loaded apps.
        }

        public static bool StartTick()
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

                    Item messageItem = new Item(playerOptionsMenu, Item.CreateData(2.0f, PREFIX + "Message", (int)ListIcons.Profile),
                        MessagePlayer, player);

                    playerOptionsMenu.AddItem(messageItem);

                    Item playerItem = new Item(screen, // Create an item that switches to the new screen.
                        Item.CreateData(2.0f, PREFIX + player.Name, (int)ListIcons.Profile, 0.0f, "CHAR_MULTIPLAYER", "CELL_999", "CELL_2000"), // Item push data.
                        ApplicationHandler.ChangeScreen, playerOptionsMenu); // Action callback and arguments.
                    screen.AddItem(playerItem); // Add the item to the launcher screen.
                }

                Item taxiServices = new Item(screen, Item.CreateData(2.0f, $"{PREFIX}Life V Taxi", 0.0f, "CHAR_TAXI", "CELL_999", "CELL_2000"), ServiceNotAvailable, "Life V Taxi", "CELL_999", "CELL_2000");
                screen.AddItem(taxiServices);

                Item towTruck = new Item(screen, Item.CreateData(2.0f, $"{PREFIX}Life V Towing", 0.0f, "CHAR_PROPERTY_TOWING_IMPOUND", "CELL_999", "CELL_2000"), ServiceNotAvailable, "Life V Towing");
                screen.AddItem(towTruck);
                Item towTruck1 = new Item(screen, Item.CreateData(2.0f, $"{PREFIX}Life V Towing1", 0.0f, "CHAR_PROPERTY_TOWING_IMPOUND", "CELL_999", "CELL_2000"), ServiceNotAvailable, "Life V Towing1");
                screen.AddItem(towTruck1);
                Item towTruck2 = new Item(screen, Item.CreateData(2.0f, $"{PREFIX}Life V Towing2", 0.0f, "CHAR_PROPERTY_TOWING_IMPOUND", "CELL_999", "CELL_2000"), ServiceNotAvailable, "Life V Towing2");
                screen.AddItem(towTruck2);
                Item towTruck3 = new Item(screen, Item.CreateData(2.0f, $"{PREFIX}Life V Towing3", 0.0f, "CHAR_PROPERTY_TOWING_IMPOUND", "CELL_999", "CELL_2000"), ServiceNotAvailable, "Life V Towing3");
                screen.AddItem(towTruck3);
                Item towTruck4 = new Item(screen, Item.CreateData(2.0f, $"{PREFIX}Life V Towing4", 0.0f, "CHAR_PROPERTY_TOWING_IMPOUND", "CELL_999", "CELL_2000"), ServiceNotAvailable, "Life V Towing4");
                screen.AddItem(towTruck4);
                Item towTruck5 = new Item(screen, Item.CreateData(2.0f, $"{PREFIX}Life V Towing5", 0.0f, "CHAR_PROPERTY_TOWING_IMPOUND", "CELL_999", "CELL_2000"), ServiceNotAvailable, "Life V Towing5");
                screen.AddItem(towTruck5);
            }
            return true;
        }

        static async void ServiceNotAvailable(dynamic[] data)
        {
            Client.TriggerEvent("curiosity:Client:Notification:LifeV", 2, $"{data[0]}", "Service Unavailable", "Sorry, we are currently unable to provide this service.", 2);
            await BaseScript.Delay(0);
        }

        static async void CallTowTruck(dynamic[] data)
        {
            Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "SMS", "Message Error", "Network Error", 2);
            await BaseScript.Delay(0);
        }

        static async void MessagePlayer(dynamic[] data)
        {
            Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "SMS", "Message Error", "Network Error", 2);
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

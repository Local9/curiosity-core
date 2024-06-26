﻿using CitizenFX.Core;
using Curiosity.Global.Shared.net.Enums.Mobile;
using Curiosity.Mobile.Client.net.Mobile.Api;

namespace Curiosity.Mobile.Client.net.Mobile.Apps
{
    class PlayerListApplication
    {
        static Application App;
        static string PREFIX = "~l~";

        static View ScreenView = View.Settings;

        static Screen screen;

        // public static Action<dynamic[]> CHANGE_THEME = new Action<dynamic[]>(ApplicationSettings.SetTheme);
        // public static Action<dynamic[]> CHANGE_WALLPAPER = new Action<dynamic[]>(ApplicationSettings.SetWallpaper);
        // public static Action<dynamic[]> CHANGE_TEXT_COLOR = new Action<dynamic[]>(ChangeTextColor);

        public static void Init()
        {

            App = new Application("Contacts", AppIcons.Contacts, 0);
            screen = new Screen(App, "Contacts", ScreenView);
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

                    Item messageItem = new Item(playerOptionsMenu, Item.CreateData(2, PREFIX + "Message", (int)ListIcons.Profile),
                        MessagePlayer, player);

                    playerOptionsMenu.AddItem(messageItem);

                    Item playerItem = new Item(screen, // Create an item that switches to the new screen.
                        Item.CreateData(0, PREFIX + player.Name), // Item push data.
                        ApplicationHandler.ChangeScreen, playerOptionsMenu); // Action callback and arguments.
                    screen.AddItem(playerItem); // Add the item to the launcher screen.
                }

                Item taxiServices = new Item(screen, Item.CreateData(0, $"{PREFIX}Life V Taxi"), ServiceNotAvailable, "Life V Taxi");
                screen.AddItem(taxiServices);

                Item towTruck = new Item(screen, Item.CreateData(0, $"{PREFIX}Life V Towing"), ServiceNotAvailable, "Life V Towing");
                screen.AddItem(towTruck);
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

using CitizenFX.Core;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Shared.Client.net.Enums.Patrol;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Police.Client.net.Classes.Menus
{
    class PoliceDispatchMenu
    {
        static Client client = Client.GetInstance();
        static Menu calloutMenu;
        // ITEMS
        static private MenuItem rbItemCode2 = new MenuItem("Code 2: Urgent - Proceed immediately");
        static private MenuItem rbItemCode3 = new MenuItem("Code 3: Emergency");
        static private MenuItem rbItemCode4 = new MenuItem("Code 4: No further assistance required");
        // SETTINGS
        static bool HasRequestedBackup = false;

        static public void Init()
        {
            client.RegisterTickHandler(OnTaskHasRequestedBackup);
        }

        static public async Task OnTaskKeyCombination()
        {
            if (ControlHelper.IsControlJustPressed(Control.Pickup, true, ControlModifier.Alt))
            {
                OpenMenu();
            }
            await Task.FromResult(0);
        }

        static async Task OnTaskHasRequestedBackup()
        {
            long gameTimer = GetGameTimer();
            while (HasRequestedBackup)
            {
                if ((GetGameTimer() - gameTimer) > 30000)
                {
                    HasRequestedBackup = false;
                    client.DeregisterTickHandler(OnTaskHasRequestedBackup);
                }
                await Client.Delay(1000);
            }
        }

        static public void OpenMenu()
        {
            MenuBaseFunctions.MenuOpen();

            MenuController.DontOpenAnyMenu = false;
            MenuController.EnableMenuToggleKeyOnController = false;

            if (calloutMenu == null)
            {
                calloutMenu = new Menu("Dispatch", "Dispatch Options");

                calloutMenu.OnMenuOpen += OnMenuOpen;
                calloutMenu.OnItemSelect += OnItemSelect;
                calloutMenu.OnMenuClose += OnMenuClose;

                MenuController.AddMenu(calloutMenu);

                MenuController.EnableMenuToggleKeyOnController = false;
            }

            calloutMenu.OpenMenu();
        }

        private static void OnMenuOpen(Menu menu)
        {
            MenuBaseFunctions.MenuOpen();
            MenuController.DontOpenAnyMenu = false;
            MenuController.EnableMenuToggleKeyOnController = false;

            menu.AddMenuItem(rbItemCode2);
            menu.AddMenuItem(rbItemCode3);
            menu.AddMenuItem(rbItemCode4);
        }

        private static void OnMenuClose(Menu menu)
        {
            calloutMenu.ClearMenuItems();
            MenuController.DontOpenAnyMenu = true;
            MenuBaseFunctions.MenuClose();
        }

        private static void OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            string streetName = "Unkown Location";

            uint streetHash = 0;
            uint streetCrossing = 0;

            GetStreetNameAtCoord(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, ref streetHash, ref streetCrossing);

            streetName = World.GetStreetName(Game.PlayerPed.Position);
            string crossing = GetStreetNameFromHashKey(streetCrossing);
            string localisedZone = World.GetZoneLocalizedName(Game.PlayerPed.Position);

            if (!string.IsNullOrEmpty(localisedZone))
            {
                streetName = $"{streetName}, {localisedZone}";
            }

            if (HasRequestedBackup)
            {
                if (menuItem == rbItemCode4)
                {
                    BaseScript.TriggerServerEvent("curiosity:Server:Player:Backup", 4, streetName);
                    HasRequestedBackup = false;
                    client.DeregisterTickHandler(OnTaskHasRequestedBackup);
                    MenuController.CloseAllMenus();
                }
                else
                {
                    Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Dispatch", $"Please wait...", "Assistance has been requested.", 2);
                }
                return;
            }

            if (menuItem == rbItemCode2)
            {
                BaseScript.TriggerServerEvent("curiosity:Server:Player:Backup", 2, streetName);
            }

            if (menuItem == rbItemCode3)
            {
                BaseScript.TriggerServerEvent("curiosity:Server:Player:Backup", 3, streetName);
            }

            if (menuItem == rbItemCode4)
            {
                BaseScript.TriggerServerEvent("curiosity:Server:Player:Backup", 4, streetName);
            }

            HasRequestedBackup = true;
            client.RegisterTickHandler(OnTaskHasRequestedBackup);
            MenuController.CloseAllMenus();
        }
    }
}

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
        static bool IsMenuOpen = false;

        static public void Init()
        {
            client.RegisterTickHandler(OnTaskHasRequestedBackup);
        }

        static public async Task OnTaskKeyCombination()
        {
            if (ControlHelper.IsControlJustPressed(Control.Pickup, true, ControlModifier.Alt))
            {
                if (!IsMenuOpen)
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
            IsMenuOpen = true;

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
            IsMenuOpen = false;
            calloutMenu.ClearMenuItems();
            MenuController.DontOpenAnyMenu = true;
            MenuBaseFunctions.MenuClose();
        }

        private static void OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            Vector3 pos = Game.PlayerPed.Position;

            if (HasRequestedBackup)
            {
                if (menuItem == rbItemCode4)
                {
                    BaseScript.TriggerServerEvent("curiosity:Server:Player:Backup", 4, pos.X, pos.Y, pos.Z);
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
                BaseScript.TriggerServerEvent("curiosity:Server:Player:Backup", 2, pos.X, pos.Y, pos.Z);
            }

            if (menuItem == rbItemCode3)
            {
                BaseScript.TriggerServerEvent("curiosity:Server:Player:Backup", 3, pos.X, pos.Y, pos.Z);
            }

            if (menuItem == rbItemCode4)
            {
                BaseScript.TriggerServerEvent("curiosity:Server:Player:Backup", 4, pos.X, pos.Y, pos.Z);
            }

            HasRequestedBackup = true;
            client.RegisterTickHandler(OnTaskHasRequestedBackup);
            MenuController.CloseAllMenus();
        }
    }
}

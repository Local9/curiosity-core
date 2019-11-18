using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using MenuAPI;
using Curiosity.Shared.Client.net.Extensions;
using CitizenFX.Core.UI;
using CitizenFX.Core.Native;

namespace Curiosity.Missions.Client.net.Scripts.Police.MenuHandler
{
    class SuspectMenu
    {
        static Client client = Client.GetInstance();

        static Menu menu;
        static bool IsMenuOpen = false;

        // Buttons
        static MenuItem mItemRequestId = new MenuItem("Request ID");
        static MenuItem mItemRunName = new MenuItem("Run Name");
        static MenuItem mItemRunPlate = new MenuItem("Run Plate");
        // Options
        static MenuItem mItemBreathalyzer = new MenuItem("Breathalyzer");
        static MenuItem mItemDrugTest = new MenuItem("Drug Test");
        static MenuItem mItemSearch = new MenuItem("Search");
        // Arrest
        static MenuItem mItemLeaveVehicle = new MenuItem("Leave Vehicle");
        static MenuItem mItemFollow = new MenuItem("Follow") { Enabled = false };
        static MenuItem mItemArrest = new MenuItem("Arrest") { Enabled = false };
        static MenuItem mItemEnterVehicle = new MenuItem("Put in car") { Enabled = false, Description = "Put ped in the back of the police car" };

        static MenuItem mItemRelease = new MenuItem("Release");

        static public void Open()
        {
            MenuController.DisableBackButton = true;
            MenuController.DontOpenAnyMenu = false;

            if (menu == null)
            {
                menu = new Menu("Suspect Menu", "Please use the options below");

                menu.OnMenuClose += Menu_OnMenuClose;
                menu.OnMenuOpen += Menu_OnMenuOpen;

                menu.OnItemSelect += Menu_OnItemSelect;

                MenuController.AddMenu(menu);
            }

            menu.OpenMenu();
        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == mItemRelease)
            {
                TrafficStop.Reset();
                menu.CloseMenu();
            }

            if (menuItem == mItemRequestId)
            {
                TrafficStop.RequestPedIdentification();
            }

            if (menuItem == mItemRunName)
            {
                TrafficStop.RunPedIdentification();
            }

            if (menuItem == mItemRunPlate)
            {
                TrafficStop.RunVehicleNumberPlate();
            }
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            menu.ClearMenuItems();
            Client.TriggerEvent("curiosity:Client:UI:LocationHide", true);
            Client.TriggerEvent("curiosity:Client:Menu:IsOpened", true);

            IsMenuOpen = true;

            // initial interactions
            menu.AddMenuItem(mItemRequestId);
            menu.AddMenuItem(mItemRunName);
            menu.AddMenuItem(mItemRunPlate);
            // release the ped
            menu.AddMenuItem(mItemRelease);
            // Interactions
            menu.AddMenuItem(mItemBreathalyzer);
            menu.AddMenuItem(mItemDrugTest);
            menu.AddMenuItem(mItemSearch);
            // Ask Ped to leave vehicle
            menu.AddMenuItem(mItemLeaveVehicle);
            menu.AddMenuItem(mItemFollow);
            menu.AddMenuItem(mItemArrest);
            menu.AddMenuItem(mItemEnterVehicle);
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            Client.TriggerEvent("curiosity:Client:UI:LocationHide", false);
            Client.TriggerEvent("curiosity:Client:Menu:IsOpened", false);
            MenuController.DontOpenAnyMenu = true;

            IsMenuOpen = false;
        }

        static public async Task OnMenuTask()
        {
            try
            {
                if (TrafficStop.StoppedDriver.Position.Distance(Game.PlayerPed.Position) < 3 && !IsMenuOpen)
                {
                    Screen.DisplayHelpTextThisFrame($"Press ~INPUT_PICKUP~ to talk with the ~b~Driver");

                    if (Game.IsControlJustPressed(0, Control.Pickup))
                    {
                        Open();
                    }
                }
                else if (TrafficStop.StoppedDriver.Position.Distance(Game.PlayerPed.Position) > 3 && IsMenuOpen)
                {
                    menu.CloseMenu();
                }
            }
            catch (Exception ex)
            {
                menu.CloseMenu();
            }
            await BaseScript.Delay(0);
        }
    }
}

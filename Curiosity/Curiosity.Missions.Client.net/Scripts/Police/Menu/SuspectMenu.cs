using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using MenuAPI;

namespace Curiosity.Missions.Client.net.Scripts.Police.MenuHandler
{
    class SuspectMenu
    {
        static Client client = Client.GetInstance();

        static Vehicle suspectVehicle;

        static Menu menu;

        // Buttons
        static MenuItem mItemRunName = new MenuItem("Request ID & Run name");
        static MenuItem mItemRelease = new MenuItem("Release");
        // Core
        static MenuItem mItemLeaveVehicle = new MenuItem("Leave Vehicle");
        static MenuItem mItemFollow = new MenuItem("Follow") { Enabled = false };
        // Options
        static MenuItem mItemBreathalyzer = new MenuItem("Breathalyzer");
        static MenuItem mItemDrugTest = new MenuItem("Drug Test");
        static MenuItem mItemSearch = new MenuItem("Search");
        // Arrest
        static MenuItem mItemArrest = new MenuItem("Arrest") { Enabled = false };
        static MenuItem mItemEnterVehicle = new MenuItem("Put in car") { Enabled = false, Description = "Put ped in the back of the police car" };

        static public void Open(Vehicle suspectVehicle)
        {
            if (suspectVehicle == null)
            {
                throw new ArgumentNullException();
            }

            SuspectMenu.suspectVehicle = suspectVehicle;

            MenuController.DisableBackButton = true;
            MenuController.DontOpenAnyMenu = false;

            client.RegisterTickHandler(OnTask);

            if (menu == null)
            {
                menu = new Menu("Suspect Menu", "Please use the options below");

                menu.OnMenuClose += Menu_OnMenuClose;
                menu.OnMenuOpen += Menu_OnMenuOpen;

                menu.OnItemSelect += Menu_OnItemSelect;
            }

            menu.OpenMenu();
        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == mItemRelease)
            {
                suspectVehicle.IsPositionFrozen = false;

                suspectVehicle.Driver.Task.ClearAll();
                suspectVehicle = null;
                menu.CloseMenu();
            }
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            menu.ClearMenuItems();
            Client.TriggerEvent("curiosity:Client:UI:LocationHide", true);
            Client.TriggerEvent("curiosity:Client:Menu:IsOpened", true);

            menu.AddMenuItem(mItemRunName);
            menu.AddMenuItem(mItemRelease);
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            Client.TriggerEvent("curiosity:Client:UI:LocationHide", false);
            Client.TriggerEvent("curiosity:Client:Menu:IsOpened", false);

            suspectVehicle.Driver.Task.ClearAll();
            suspectVehicle.MarkAsNoLongerNeeded();

            MenuController.DontOpenAnyMenu = true;
        }

        static async Task OnTask()
        {
            try
            {
                if (suspectVehicle.IsDead)
                {

                }

                if (suspectVehicle == null)
                {
                    client.DeregisterTickHandler(OnTask);
                }
            }
            catch (Exception ex)
            {
                client.DeregisterTickHandler(OnTask);
            }
            await BaseScript.Delay(0);
        }
    }
}

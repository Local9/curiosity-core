using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using MenuAPI;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net.Enums.Patrol;
using CitizenFX.Core.UI;
using CitizenFX.Core.Native;

namespace Curiosity.Missions.Client.net.Scripts.Police.MenuHandler
{
    class SuspectMenu
    {
        static Client client = Client.GetInstance();

        static public Menu TrafficStopMenu;
        static public bool IsMenuOpen = false;

        static List<string> CitationPrices = new List<string>() { "$50", "$100", "$150", "$200", "$250", "$500", "$1000" };

        // Initial Menu
        static MenuListItem mListItemSpeech = new MenuListItem("Speech", Enum.GetNames(typeof(SpeechType)).ToList(), 0);
        static MenuItem mItemHello = new MenuItem("Hello");
        static MenuItem mItemRequestId = new MenuItem("Ask for Identification");

        static MenuItem mItemRelease = new MenuItem("Release");


        static MenuItem mItemRunName = new MenuItem("Run Name");
        static MenuItem mItemRunPlate = new MenuItem("Run Plate");
        static MenuListItem mItemIssueTicket = new MenuListItem("Issue Ticket", CitationPrices, 0) { Description = "~w~Press ~r~ENTER ~w~to issue the ~b~Citation~w~." };
        static MenuItem mItemIssueWarning = new MenuItem("Issue Warning");
        static MenuItem mItemBreathalyzer = new MenuItem("Breathalyzer");
        static MenuItem mItemDrugTest = new MenuItem("Drug Test");
        static MenuItem mItemSearch = new MenuItem("Search");
        static MenuItem mItemFollow = new MenuItem("Follow") { Enabled = false };
        static MenuItem mItemArrest = new MenuItem("Arrest") { Enabled = false };
        static MenuItem mItemOrderBackInCar = new MenuItem("Order back in Vehicle");

        // static MenuItem mItemEnterVehicle = new MenuItem("Put in car") { Enabled = false, Description = "Put ped in the back of the police car" };


        static public void Open()
        {
            MenuController.DontOpenAnyMenu = false;
            MenuController.DisableBackButton = true;

            if (TrafficStopMenu == null)
            {
                TrafficStopMenu = new Menu("", "Suspect Interactions") { };

                TrafficStopMenu.OnMenuClose += Menu_OnMenuClose;
                TrafficStopMenu.OnMenuOpen += Menu_OnMenuOpen;
                TrafficStopMenu.OnItemSelect += Menu_OnItemSelect;
                TrafficStopMenu.OnListIndexChange += Menu_OnListIndexChange;

                MenuController.AddMenu(TrafficStopMenu);
            }

            TrafficStopMenu.OpenMenu();
        }

        private static void Menu_OnListIndexChange(Menu menu, MenuListItem listItem, int oldSelectionIndex, int newSelectionIndex, int itemIndex)
        {
            if (listItem == mListItemSpeech)
            {
                Client.speechType = (SpeechType)newSelectionIndex;
            }
        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == mItemRelease)
            {
                TrafficStop.InteractionRelease();
                menu.CloseMenu();
            }

            if (menuItem == mItemHello)
            {
                TrafficStop.InteractionHello();
            }

            if (menuItem == mItemRequestId)
            {
                TrafficStop.InteractionRequestPedIdentification();
                mItemRunName.Enabled = true;
            }

            if (menuItem == mItemRunName)
            {
                TrafficStop.InteractionRunPedIdentification();
            }

            if (menuItem == mItemRunPlate)
            {
                TrafficStop.InteractionRunVehicleNumberPlate();
            }

            if (menuItem == mItemIssueWarning)
            {
                TrafficStop.InteractionIssueWarning();
                menu.CloseMenu();
            }
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            menu.ClearMenuItems();

            Client.TriggerEvent("curiosity:Client:UI:LocationHide", true);
            Client.TriggerEvent("curiosity:Client:Menu:IsOpened", true);

            IsMenuOpen = true;

            menu.AddMenuItem(mListItemSpeech);
            menu.AddMenuItem(mItemHello);
            menu.AddMenuItem(mItemRequestId);
            
            mItemRunName.Enabled = false;

            menu.AddMenuItem(mItemRunName);
            menu.AddMenuItem(mItemRunPlate);

            Submenu.TrafficStopQuestions.SetupMenu();
            Submenu.TrafficStopInteractions.SetupMenu();

            menu.AddMenuItem(mItemIssueWarning);
            
            menu.AddMenuItem(mItemRelease);
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            Client.TriggerEvent("curiosity:Client:UI:LocationHide", false);
            Client.TriggerEvent("curiosity:Client:Menu:IsOpened", false);
            MenuController.DontOpenAnyMenu = true;
        }

        static public async Task OnMenuTask()
        {
            try
            {
                if (TrafficStop.StoppedDriver.Position.Distance(Game.PlayerPed.Position) < 3 && !IsMenuOpen && !Game.PlayerPed.IsInVehicle())
                {
                    Screen.DisplayHelpTextThisFrame($"Press ~INPUT_PICKUP~ to talk with the ~b~Driver");

                    if (Game.IsControlJustPressed(0, Control.Pickup))
                    {
                        Open();
                    }
                }
                else if (TrafficStop.StoppedDriver.Position.Distance(Game.PlayerPed.Position) >= 4 && IsMenuOpen)
                {
                    TrafficStopMenu.CloseMenu();
                    IsMenuOpen = false;
                }

                if (TrafficStop.StoppedDriver.IsDead)
                {
                    client.DeregisterTickHandler(OnMenuTask);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnMenuTask -> {ex}");
            }
            await BaseScript.Delay(0);
        }

        public static void AddSubMenu(Menu menu, Menu submenu, string label = "→→→", bool buttonEnabled = true)
        {
            MenuController.AddSubmenu(menu, submenu);
            MenuItem submenuButton = new MenuItem(submenu.MenuTitle, submenu.MenuSubtitle) { Label = label, Enabled = buttonEnabled };
            menu.AddMenuItem(submenuButton);
            MenuController.BindMenuItem(menu, submenu, submenuButton);
        }
    }
}

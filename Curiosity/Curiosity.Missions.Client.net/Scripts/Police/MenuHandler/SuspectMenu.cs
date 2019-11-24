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
        static MenuType menuTypeToOpen;

        static List<string> CitationPrices = new List<string>() { "$50", "$100", "$150", "$200", "$250", "$500", "$1000" };

        // Initial Menu
        static MenuListItem mListItemSpeech = new MenuListItem("Speech", Enum.GetNames(typeof(SpeechType)).ToList(), 0);
        static MenuItem mItemHello = new MenuItem("Hello");
        static MenuItem mItemRequestId = new MenuItem("Ask for Identification");
        static MenuItem mItemCoroner = new MenuItem("Coroner");
        static MenuItem mItemTowService = new MenuItem("Tow Service");
        static MenuItem mItemRelease = new MenuItem("Release");

        static MenuItem mItemRunName = new MenuItem("Run Name");
        static MenuItem mItemRunPlate = new MenuItem("Run Plate");
        static MenuListItem mItemIssueTicket = new MenuListItem("Issue Ticket", CitationPrices, 0) { Description = "~w~Press ~r~ENTER ~w~to issue the ~b~Citation~w~." };
        static MenuItem mItemIssueWarning = new MenuItem("Issue Warning");

        static public void Open(MenuType menuType)
        {
            menuTypeToOpen = menuType;
            MenuController.DontOpenAnyMenu = false;
            MenuController.DisableBackButton = true;

            if (TrafficStopMenu == null)
            {
                TrafficStopMenu = new Menu("", "Suspect Menu") { };

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

            if (menuItem == mItemTowService)
            {
                Extras.VehicleTow.RequestService();
                menu.CloseMenu();
            }

            if (menuItem == mItemCoroner)
            {
                Extras.Coroner.RequestService();
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

            if (menuTypeToOpen == MenuType.Normal)
            {
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

            if (menuTypeToOpen == MenuType.Vehicle)
            {
                menu.AddMenuItem(mItemTowService);
            }

            if (menuTypeToOpen == MenuType.DeadPed)
            {
                menu.AddMenuItem(mItemCoroner);
            }
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
                // If ped is dead then coroner option
                // If vehicle is empty and ped is dead or arrested, allow tow
                // else interact with the ped

                if (TrafficStop.StoppedDriver.IsDead)
                {
                    if (TrafficStop.TargetVehicle.Position.Distance(Game.PlayerPed.Position) < 3 && !IsMenuOpen)
                    {
                        Screen.DisplayHelpTextThisFrame($"Press ~INPUT_PICKUP~ to open ~b~interaction menu");

                        if (Game.IsControlJustPressed(0, Control.Pickup))
                        {
                            Open(MenuType.Vehicle);
                        }
                    }
                    else if (TrafficStop.StoppedDriver.Position.Distance(Game.PlayerPed.Position) < 3 && !IsMenuOpen)
                    {
                        Screen.DisplayHelpTextThisFrame($"Press ~INPUT_PICKUP~ to open ~b~interaction menu");

                        if (Game.IsControlJustPressed(0, Control.Pickup))
                        {
                            Open(MenuType.DeadPed);
                        }
                    }
                }
                else
                {
                    if (TrafficStop.StoppedDriver.Position.Distance(Game.PlayerPed.Position) < 3 && !IsMenuOpen && !Game.PlayerPed.IsInVehicle())
                    {
                        Screen.DisplayHelpTextThisFrame($"Press ~INPUT_PICKUP~ to talk with the ~b~Driver");

                        if (Game.IsControlJustPressed(0, Control.Pickup))
                        {
                            Open(MenuType.Normal);
                        }
                    }
                    else if (TrafficStop.StoppedDriver.Position.Distance(Game.PlayerPed.Position) >= 4 && IsMenuOpen)
                    {
                        TrafficStopMenu.CloseMenu();
                        IsMenuOpen = false;
                    }
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

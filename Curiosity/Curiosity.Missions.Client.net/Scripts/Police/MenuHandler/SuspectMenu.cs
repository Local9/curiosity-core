using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Shared.Client.net.Enums.Patrol;
using Curiosity.Shared.Client.net.Extensions;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        static MenuItem mItemRemoveHandcuff = new MenuItem("Remove Handcuffs");
        static MenuItem mItemHandcuff = new MenuItem("Handcuff");
        static MenuItem mItemGrab = new MenuItem("Grab Suspect");

        static MenuItem mItemRunName = new MenuItem("Run Name");
        static MenuItem mItemRunPlate = new MenuItem("Run Plate");
        static MenuListItem mItemIssueTicket = new MenuListItem("Issue Ticket", CitationPrices, 0) { Description = "~w~Press ~r~ENTER ~w~to issue the ~b~Citation~w~." };
        static MenuItem mItemIssueWarning = new MenuItem("Issue Warning");

        static MenuItem mItemCloseMenu = new MenuItem("Close Menu");

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
                client.DeregisterTickHandler(MenuHandler.SuspectMenu.OnMenuTask);
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
                client.DeregisterTickHandler(MenuHandler.SuspectMenu.OnMenuTask);
            }

            if (menuItem == mItemHandcuff)
            {
                ArrestPed.InteractionHandcuff();
                mItemRemoveHandcuff.Enabled = true;
                mItemGrab.Enabled = true;
            }

            if (menuItem == mItemRemoveHandcuff)
            {
                ArrestPed.InteractionHandcuff();
                mItemRemoveHandcuff.Enabled = false;
                mItemGrab.Enabled = false;
            }

            if (menuItem == mItemGrab)
            {
                ArrestPed.InteractionGrab();
            }

            if (menuItem == mItemCloseMenu)
            {
                MenuController.CloseAllMenus();
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
                MenuController.DisableBackButton = true;
                menu.AddMenuItem(mListItemSpeech);

                if (ArrestPed.IsPedBeingArrested)
                {
                    menu.AddMenuItem(mItemHandcuff);
                    mItemRemoveHandcuff.Enabled = ArrestPed.IsPedCuffed;
                    menu.AddMenuItem(mItemRemoveHandcuff);
                    mItemGrab.Enabled = ArrestPed.IsPedCuffed;
                    menu.AddMenuItem(mItemGrab);
                }

                menu.AddMenuItem(mItemHello);
                menu.AddMenuItem(mItemRequestId);
                mItemRunName.Enabled = false;
                menu.AddMenuItem(mItemRunName);
                menu.AddMenuItem(mItemRunPlate);
                Submenu.TrafficStopQuestions.SetupMenu();
                Submenu.TrafficStopInteractions.SetupMenu();
                menu.AddMenuItem(mItemIssueWarning);
                menu.AddMenuItem(mItemRelease);
                menu.AddMenuItem(mItemCloseMenu);
            }

            if (menuTypeToOpen == MenuType.Vehicle)
            {
                MenuController.DisableBackButton = false;
                menu.AddMenuItem(mItemTowService);
            }

            if (menuTypeToOpen == MenuType.DeadPed)
            {
                MenuController.DisableBackButton = false;
                menu.AddMenuItem(mItemCoroner);
            }
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
                // If ped is dead then coroner option
                // If vehicle is empty and ped is dead or arrested, allow tow
                // else interact with the ped

                if (TrafficStop.TargetVehicle != null)
                {
                    if (TrafficStop.TargetVehicle.Position.Distance(Game.PlayerPed.Position) < 3 && !IsMenuOpen && (TrafficStop.StoppedDriver.IsDead || TrafficStop.TargetVehicle.IsDead))
                    {
                        Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to open ~b~vehicle interaction menu");

                        if (Game.IsControlJustPressed(0, Control.Context))
                        {
                            Open(MenuType.Vehicle);
                            await Client.Delay(100);
                        }
                    }

                    if (TrafficStop.TargetVehicle.Position.Distance(Game.PlayerPed.Position) < 3 && !IsMenuOpen && ArrestPed.IsPedCuffed && TrafficStop.StoppedDriver.CurrentVehicle == Client.CurrentVehicle)
                    {
                        Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to open ~b~vehicle interaction menu");

                        if (Game.IsControlJustPressed(0, Control.Context))
                        {
                            Open(MenuType.Vehicle);
                            await Client.Delay(100);
                        }
                    }
                }

                if (TrafficStop.StoppedDriver != null)
                {
                    if (TrafficStop.StoppedDriver.Position.Distance(Game.PlayerPed.Position) < 3 && !IsMenuOpen && TrafficStop.StoppedDriver.IsDead)
                    {
                        Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to open ~b~ped interaction menu");

                        if (Game.IsControlJustPressed(0, Control.Context))
                        {
                            Open(MenuType.DeadPed);
                            await Client.Delay(100);
                        }
                    }

                    if (TrafficStop.StoppedDriver.Position.Distance(Game.PlayerPed.Position) < 3 && !IsMenuOpen && !Game.PlayerPed.IsInVehicle() && TrafficStop.StoppedDriver.IsAlive)
                    {
                        Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to talk with the ~b~Driver");

                        if (Game.IsControlJustPressed(0, Control.Context))
                        {
                            Open(MenuType.Normal);
                            await Client.Delay(100);
                        }
                    }

                    if (TrafficStop.StoppedDriver.Position.Distance(Game.PlayerPed.Position) >= 30 && IsMenuOpen)
                    {
                        TrafficStopMenu.CloseMenu();
                        IsMenuOpen = false;
                    }
                }

                if (TrafficStop.StoppedDriver == null && TrafficStop.TargetVehicle == null)
                {
                    LogMessage("Closing menu, both Driver and Vehicle are null");
                    IsMenuOpen = false;
                    client.DeregisterTickHandler(OnMenuTask);
                    TrafficStop.Reset();
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

        static public void LogMessage(string msg)
        {
            if (Classes.PlayerClient.ClientInformation.IsDeveloper())
            {
                Debug.WriteLine($"[LOG] -> {msg}");
            }
        }
    }
}

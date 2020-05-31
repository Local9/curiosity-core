using CitizenFX.Core;
using MenuAPI;
using System;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Menus.Client.net.Classes.Menus
{
    class MenuBase
    {
        private const string PERSONAL_VEHICLE_KEY = "PERSONAL_VEHICLE_ID";
        static Client client = Client.GetInstance();

        public static Menu Menu = new Menu("Interaction Menu", "Player Interaction Menu");
        public static bool isMenuOpen = Menu.Visible;

        public static MenuItem showForum = new MenuItem("Open Player Guides", "Select this to view our player guides") { LeftIcon = MenuItem.Icon.GLOBE_BLUE };

        public static MenuItem showPoliceMenu = new MenuItem("Police Options", "Options for the Police Activities") { LeftIcon = MenuItem.Icon.MISSION_STAR };
        static bool AddedPoliceOptions = false;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Player:Menu:VehicleId", new Action<int>(OnVehicleId));

            int existingVehicleId = GetResourceKvpInt(PERSONAL_VEHICLE_KEY);

            if (existingVehicleId > 0)
            {
                OnVehicleId(existingVehicleId);
            }

            MenuController.MenuAlignment = MenuController.MenuAlignmentOption.Left;
            
            Menu.OnItemSelect += (_menu, _item, _index) =>
            {
                if (_item == showForum)
                {
                    Client.TriggerEvent("curiosity:Client:Interface:ShowForum");
                    _menu.CloseMenu();
                }

                if (_item == showPoliceMenu)
                {
                    Client.TriggerEvent("curiosity:Client:Police:ShowOptions");
                    _menu.CloseMenu();
                }
            };

            Menu.OnMenuClose += (_menu) =>
            {
                Client.TriggerEvent("curiosity:Client:UI:LocationHide", false);
                Client.TriggerEvent("curiosity:Client:Menu:IsOpened", false);
            };

            Menu.OnMenuOpen += (_menu) =>
            {
                Client.TriggerEvent("curiosity:Client:UI:LocationHide", true);
                Client.TriggerEvent("curiosity:Client:Menu:IsOpened", true);


                if (Player.PlayerInformation.Job == Global.Shared.net.Enums.Job.Police && !AddedPoliceOptions)
                {
                    Menu.AddMenuItem(showPoliceMenu);
                    AddedPoliceOptions = true;
                }

                if (Player.PlayerInformation.Job != Global.Shared.net.Enums.Job.Police)
                {
                    Menu.RemoveMenuItem(showPoliceMenu);
                    AddedPoliceOptions = false;
                }
            };

            Menu.AddMenuItem(showForum);

            MenuController.AddMenu(Menu);

            //// Classes.Menus.Inventory.Init();
            Classes.Menus.PlayerMenu.Init();
            Classes.Menus.PlayerCreator.PlayerCreatorMenu.Init();
            Classes.Menus.PlayerCreator.PlayerOverlays.Init();
            Classes.Menus.PlayerCreator.PlayerComponents.Init();
            Classes.Menus.PlayerCreator.PlayerProps.Init();
            Classes.Menus.PlayerCreator.PlayerReset.Init();
            Classes.Menus.PlayerCreator.PlayerSave.Init();
            //// ONLINE PLAYER MENU ITEMS
            Classes.Menus.OnlinePlayers.Init();
            Classes.Menus.PlayerInteractions.ReportInteraction.Init();
            Classes.Menus.PlayerInteractions.KickInteraction.Init();
            Classes.Menus.PlayerInteractions.BanInteraction.Init();
            //// VEHICLE
            Classes.Menus.VehicleMenu.Init();
            // DONATOR
            Classes.Menus.Donator.Init();

            // Tools
            Classes.Menus.Developer.Init();
            // Classes.Menus.MissionCreator.MissionMenu.Init();
        }

        public static void MenuOpen(bool isOpen)
        {
            //if (Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER)
            //{
            //    Debug.WriteLine($"MenuOpen: {isOpen}");
            //}

            if (isOpen)
            {
                Client.TriggerEvent("curiosity:Client:UI:LocationHide", true);
                Client.TriggerEvent("curiosity:Client:Menu:IsOpened", true);
            }
            else
            {
                Client.TriggerEvent("curiosity:Client:UI:LocationHide", false);
                Client.TriggerEvent("curiosity:Client:Menu:IsOpened", false);
            }
        }

        public static void AddMenuItem(MenuItem menuItem)
        {
            Menu.AddMenuItem(menuItem);
        }

        public static void AddSubMenu(Menu submenu)
        {
            AddSubMenu(Menu, submenu);
        }

        public static MenuItem AddSubMenu(Menu submenu, string label = "→→→", bool buttonEnabled = true, string description = "", MenuItem.Icon leftIcon = MenuItem.Icon.NONE, MenuItem.Icon rightIcon = MenuItem.Icon.NONE)
        {
            return AddSubMenu(Menu, submenu, label, buttonEnabled, description, leftIcon, rightIcon);
        }

        public static MenuItem AddSubMenu(Menu menu, Menu submenu, string label = "→→→", bool buttonEnabled = true, string description = "", MenuItem.Icon leftIcon = MenuItem.Icon.NONE, MenuItem.Icon rightIcon = MenuItem.Icon.NONE)
        {
            MenuController.AddSubmenu(menu, submenu);
            MenuItem submenuButton = new MenuItem(submenu.MenuTitle, submenu.MenuSubtitle) { Label = label, LeftIcon = leftIcon, RightIcon = rightIcon };
            if (!buttonEnabled)
            {
                submenuButton = new MenuItem(submenu.MenuTitle, description) { Enabled = buttonEnabled, RightIcon = MenuItem.Icon.LOCK };
            }
            menu.AddMenuItem(submenuButton);
            MenuController.BindMenuItem(menu, submenu, submenuButton);
            return submenuButton;
        }

        public static void RemoveMenu(Menu menu)
        {
            foreach (MenuItem menuItem in Menu.GetMenuItems())
            {
                if (menu.MenuTitle == menuItem.Text)
                    Menu.RemoveMenuItem(menuItem);
            }
        }

        public static void OnVehicleId(int vehicleId)
        {
            if (Client.CurrentVehicle == null)
            {
                SetResourceKvpInt(PERSONAL_VEHICLE_KEY, vehicleId);
                Client.CurrentVehicle = new Vehicle(vehicleId);
            }
            else if (Client.CurrentVehicle.Handle != vehicleId)
            {
                SetResourceKvpInt(PERSONAL_VEHICLE_KEY, vehicleId);
                Client.CurrentVehicle = new Vehicle(vehicleId);
            }
        }
    }
}

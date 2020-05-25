using CitizenFX.Core;
using Curiosity.Menus.Client.net.Classes.Data;
using Curiosity.Menus.Client.net.Classes.Player;
using Curiosity.Menus.Client.net.Classes.Scripts;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Menus.Client.net.Classes.Menus
{
    class Donator
    {
        static Client client = Client.GetInstance();

        static Menu donatorMenu;
        static MenuItem subMenuItem;
        static MenuItem menuItemDonatorVehicles = new MenuItem("Vehicles") { LeftIcon = MenuItem.Icon.INV_CAR };
        static MenuItem menuItemRemoveCompanion = new MenuItem("Remove Companion") { };

        static bool devConfigured = false;

        static List<CompanionData> companions = new List<CompanionData>()
        {
            new CompanionData("Husky", PedHash.Husky, canInteract: true), // retriever
            new CompanionData("Poodle", PedHash.Poodle, canInteract: true), // retriever ?
            new CompanionData("Pug", PedHash.Pug),
            new CompanionData("Retriever", PedHash.Retriever, canInteract: true), // retriever
            new CompanionData("Rottweiler", PedHash.Rottweiler, canInteract: true), // rottweiller
            new CompanionData("Shepherd", PedHash.Shepherd, canInteract: true), // retriever
            new CompanionData("Cat", PedHash.Cat),
            new CompanionData("Chimp", PedHash.Chimp, true),
            new CompanionData("Westy", PedHash.Westy),
            new CompanionData("Cop: Female", PedHash.Cop01SFY, true),
            new CompanionData("Cop: Male", PedHash.Cop01SMY, true),
            new CompanionData("Sheriff: Female", PedHash.Sheriff01SFY, true),
            new CompanionData("Sheriff: Male", PedHash.Sheriff01SMY, true),
        };

        static int selectedCompanion = 0;

        static MenuListItem menuListItemCompanion = new MenuListItem("Companion", companions.Select(x => x.Label).ToList(), selectedCompanion);

        public static void Init()
        {
            client.RegisterEventHandler("playerSpawned", new Action<dynamic>(OnPlayerSpawned));
            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));
            SetupMenu();
        }

        static void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;
            SetupMenu();
        }

        static void OnPlayerSpawned(dynamic dynData)
        {
            SetupMenu();
        }

        static void SetupMenu()
        {
            bool isItemEnabled = (Player.PlayerInformation.IsStaff() || Player.PlayerInformation.IsDonator());
            string description = isItemEnabled ? "Donator Options" : "~b~Pateron: ~y~https://www.patreon.com/lifev\n\n~w~If you have recently donated, please reconnect or contact support.";
            if (donatorMenu == null)
            {
                donatorMenu = new Menu("Donator Menu", "Thank you for your support!");

                donatorMenu.OnMenuOpen += Menu_OnMenuOpen;
                donatorMenu.OnMenuClose += Menu_OnMenuClose;

                donatorMenu.OnItemSelect += Menu_OnItemSelect;
                donatorMenu.OnListItemSelect += DonatorMenu_OnListItemSelect;

                subMenuItem = MenuBase.AddSubMenu(donatorMenu, "→→→", isItemEnabled, description, MenuItem.Icon.STAR);
            }

            subMenuItem.Enabled = isItemEnabled;

            MenuController.MainMenu.GetMenuItems().ForEach(mitem =>
            {
                if (mitem.Text == donatorMenu.MenuTitle)
                {
                    mitem.Enabled = isItemEnabled;
                    mitem.Description = description;
                    mitem.Label = "→→→";
                    mitem.RightIcon = MenuItem.Icon.NONE;
                    mitem.LeftIcon = MenuItem.Icon.STAR;
                };
            });
        }

        private static void DonatorMenu_OnListItemSelect(Menu menu, MenuListItem listItem, int selectedIndex, int itemIndex)
        {
            if (listItem == menuListItemCompanion)
            {
                CompanionData selection = companions[selectedIndex];
                Companion.CreateCompanion(selection);
            }
        }

        private static void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            if (menuItem == menuItemDonatorVehicles)
            {
                Client.TriggerEvent("curiosity:Client:Vehicle:OpenDonatorVehicles");
                menu.CloseMenu();
            }

            if (menuItem == menuItemRemoveCompanion)
            {
                Companion.RemoveCompanion();
            }
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            MenuBase.MenuOpen(false);
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            MenuBase.MenuOpen(true);

            bool isItemEnabled = (Player.PlayerInformation.IsStaff() || Player.PlayerInformation.IsDonator());
            subMenuItem.Enabled = isItemEnabled;

            if (PlayerInformation.IsDeveloper() || PlayerInformation.IsProjectManager())
            {
                if (!devConfigured)
                {
                    companions.Add(new CompanionData("DEV: Cow", PedHash.Cow));
                    companions.Add(new CompanionData("DEV: Mountain Lion", PedHash.MountainLion));
                    companions.Add(new CompanionData("DEV: Chiken Hawk", PedHash.ChickenHawk));
                    companions.Add(new CompanionData("DEV: Gull", PedHash.Seagull));
                    companions.Add(new CompanionData("DEV: Boar", PedHash.Boar));
                    companions.Add(new CompanionData("DEV: Coyote", PedHash.Coyote));
                    companions.Add(new CompanionData("DEV: Hen", PedHash.Hen));
                    companions.Add(new CompanionData("DEV: Deer", PedHash.Deer));
                    companions.Add(new CompanionData("DEV: Pig", PedHash.Pig));
                    companions.Add(new CompanionData("DEV: Rabbit", PedHash.Rabbit));
                    companions.Add(new CompanionData("DEV: Rat", PedHash.Rat));
                    companions.Add(new CompanionData("DEV: Pidgen", PedHash.Pigeon));
                    companions.Add(new CompanionData("DEV: Franklin", PedHash.Franklin, true));
                    companions.Add(new CompanionData("DEV: Trevor", PedHash.Trevor, true));
                    companions.Add(new CompanionData("DEV: Michael", PedHash.Michael, true));
                    devConfigured = true;
                }

                menuListItemCompanion.ListItems.Clear();
                menuListItemCompanion.ListItems = companions.Select(x => x.Label).ToList();
                menuListItemCompanion.ListIndex = selectedCompanion;
            }

            menu.ClearMenuItems();
            menu.AddMenuItem(menuItemDonatorVehicles);
            menu.AddMenuItem(menuListItemCompanion);
            menu.AddMenuItem(menuItemRemoveCompanion);
        }
    }
}
